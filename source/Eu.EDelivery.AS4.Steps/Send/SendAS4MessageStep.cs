using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Send <see cref="AS4Message" /> to the corresponding Receiving MSH
    /// </summary>
    [Info("Send AS4 Message to the configured receiver")]
    [Description("This step makes sure that an AS4 Message that has been processed, is sent to its destination")]
    public class SendAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createDatastore;
        private readonly IHttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class
        /// </summary>
        public SendAS4MessageStep() 
            : this(Registry.Instance.CreateDatastoreContext, new ReliableHttpClient()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep"/> class.
        /// </summary>
        /// <param name="createDatastore">Delegate to create a new datastore context.</param>
        public SendAS4MessageStep(Func<DatastoreContext> createDatastore) 
            : this(createDatastore, new ReliableHttpClient()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class.
        /// </summary>
        /// <param name="createDatastore">Delegate to create a new datastore context.</param>
        /// <param name="client">Instance to handle the HTTP response.</param>
        internal SendAS4MessageStep(Func<DatastoreContext> createDatastore, IHttpClient client)
        {
            if (createDatastore == null)
            {
                throw new ArgumentNullException(nameof(createDatastore));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _createDatastore = createDatastore;
            _httpClient = client;
        }

        /// <summary>
        /// Send the <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.ReceivedMessage == null && messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SendAS4MessageStep)} requires a MessagingContext with a ReceivedStream or an AS4Message to correctly send the message");
            }

            if (messagingContext.ReceivedMessage == null && messagingContext.AS4Message.IsPullRequest == false)
            {
                throw new InvalidOperationException(
                    $"{nameof(SendAS4MessageStep)} expects a PullRequest AS4Message when the MessagingContext does not contain a ReceivedStream");
            }

            PushConfiguration pushConfig = GetPushConfiguration(messagingContext.SendingPMode, messagingContext.ReceivingPMode);
            if (pushConfig?.Protocol?.Url == null)
            {
                throw new ConfigurationErrorsException(
                    "Message cannot be send because neither the Sending or Receiving PMode has a Protocol.Url child in a <PushConfiguration/> or <ResponseConfiguration/> element");
            }

            AS4Message as4Message = await DeserializeUnderlyingStreamIfPresentAsync(
                messagingContext.ReceivedMessage,
                otherwise: messagingContext.AS4Message);

            try
            {
                string contentType = messagingContext.ReceivedMessage?.ContentType ?? messagingContext.AS4Message.ContentType;
                HttpWebRequest request = CreateWebRequest(pushConfig, contentType.Replace("charset=\"utf-8\"", ""));

                await WriteToHttpRequestStreamAsync(request, messagingContext).ConfigureAwait(false);
                messagingContext.ModifyContext(as4Message);

                return await HandleHttpResponseAsync(request, messagingContext).ConfigureAwait(false);
            }
            catch
            {
                await UpdateRetryStatusForMessageAsync(messagingContext, SendResult.RetryableFail);
                return StepResult.Failed(messagingContext).AndStopExecution();
            }
        }

        private static PushConfiguration GetPushConfiguration(
            SendingProcessingMode sendingPMode,
            ReceivingProcessingMode receivingPMode)
        {
            if (sendingPMode != null)
            {
                Logger.Trace($"Use SendingPMode {sendingPMode.Id} for sending the AS4Message");
                return sendingPMode.PushConfiguration;
            }

            Logger.Trace($"Use ReceivingPMode {receivingPMode.Id} for sending the AS4Message");
            return receivingPMode?.ReplyHandling?.ResponseConfiguration;
        }

        private static async Task<AS4Message> DeserializeUnderlyingStreamIfPresentAsync(ReceivedMessage rm, AS4Message otherwise)
        {
            if (rm != null)
            {
                rm.UnderlyingStream.Position = 0;

                AS4Message as4Message =
                    await SerializerProvider
                        .Default
                        .Get(rm.ContentType)
                        .DeserializeAsync(
                              rm.UnderlyingStream,
                              rm.ContentType)
                        .ConfigureAwait(false);

                // TODO: the serializer already does this?
                rm.UnderlyingStream.Position = 0;

                return as4Message;
            }

            return otherwise;
        }

        private HttpWebRequest CreateWebRequest(PushConfiguration pushConfig, string contentType)
        {
            string url = pushConfig.Protocol.Url;
            Logger.Trace($"Creating WebRequest to {url}");

            HttpWebRequest request = _httpClient.Request(url, contentType);
            X509Certificate2 clientCert = RetrieveClientCertificate(pushConfig.TlsConfiguration);
            if (clientCert != null)
            {
                request.ClientCertificates.Add(clientCert);
            }

            return request;
        }

        private static X509Certificate2 RetrieveClientCertificate(TlsConfiguration tlsConfig)
        {
            if (tlsConfig == null
                || tlsConfig.IsEnabled == false
                || tlsConfig.ClientCertificateInformation == null)
            {
                return null;
            }

            Logger.Trace("Adding Client TLS Certificate to HTTP Request");
            X509Certificate2 certificate = RetrieveTlsCertificate(tlsConfig);
            if (certificate == null)
            {
                throw new NotSupportedException(
                    "The TLS certificate information specified in the PMode could not be used to retrieve the certificate");
            }

            return certificate;
        }

        private static X509Certificate2 RetrieveTlsCertificate(TlsConfiguration configuration)
        {
            if (configuration.ClientCertificateInformation is ClientCertificateReference clientCertRef)
            {
                return Registry.Instance.CertificateRepository.GetCertificate(
                    clientCertRef.ClientCertificateFindType,
                    clientCertRef.ClientCertificateFindValue);
            }

            if (configuration.ClientCertificateInformation is PrivateKeyCertificate embeddedCertInfo)
            {
                return new X509Certificate2(
                    rawData: Convert.FromBase64String(embeddedCertInfo.Certificate),
                    password: embeddedCertInfo.Password,
                    keyStorageFlags: X509KeyStorageFlags.Exportable);
            }

            return null;
        }

        private static async Task WriteToHttpRequestStreamAsync(HttpWebRequest request, MessagingContext ctx)
        {
            try
            {
                long messageSize =
                    ctx.ReceivedMessage?.UnderlyingStream?.CanSeek ?? false
                        ? ctx.ReceivedMessage.UnderlyingStream.Length
                        : ctx.AS4Message?.DetermineMessageSize() ?? 0L;
                
                if (messageSize >= 0)
                {
                    request.ContentLength = messageSize;
                }

                request.AllowWriteStreamBuffering = false;

                Logger.Debug($"Send AS4Message to {request.RequestUri}");
                using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    if (ctx.ReceivedMessage != null)
                    {
                        await ctx.ReceivedMessage
                                 .UnderlyingStream
                                 .CopyToFastAsync(requestStream)
                                 .ConfigureAwait(false);
                    }
                    else
                    {
                        await SerializerProvider
                            .Default
                            .Get(request.ContentType)
                            .SerializeAsync(ctx.AS4Message, requestStream);
                    }
                }
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure
                    && (ctx.AS4Message?.IsPullRequest ?? false))
                {
                    Logger.Trace($"The PullRequest could not be send to {request.RequestUri} due to a WebException");
                    Logger.Trace(exception.Message);
                }

                Logger.ErrorDeep(exception);
                throw new WebException($"Failed to Send AS4Message to Url: {request.RequestUri}.", exception);
            }
        }

        private async Task<StepResult> HandleHttpResponseAsync(HttpWebRequest request, MessagingContext ctx)
        {
            Logger.Trace($"AS4Message received from: {request.Address}");
            (HttpWebResponse webResponse, WebException exception) =
                await _httpClient.Respond(request)
                                 .ConfigureAwait(false);

            if (webResponse != null
                && ContentTypeSupporter.IsContentTypeSupported(webResponse.ContentType))
            {
                using (AS4Response res = await AS4Response.Create(ctx, webResponse).ConfigureAwait(false))
                {
                    SendResult result = SendResultUtils.DetermineSendResultFromHttpResonse(res.StatusCode);
                    await UpdateRetryStatusForMessageAsync(ctx, result);

                    var handler = new PullRequestResponseHandler(
                        _createDatastore,
                        new EmptyBodyResponseHandler(
                            new TailResponseHandler()));

                    return await handler
                        .HandleResponse(res)
                        .ConfigureAwait(false);
                }
            }

            Logger.ErrorDeep(exception);
            throw new WebException($"Failed to Send AS4Message to Url: {request.RequestUri}.", exception);
        }

        private async Task UpdateRetryStatusForMessageAsync(MessagingContext ctx, SendResult result)
        {
            if (ctx.MessageEntityId.HasValue)
            {
                using (DatastoreContext db = _createDatastore())
                {
                    var repository = new DatastoreRepository(db);
                    var service = new MarkForRetryService(repository);
                    service.UpdateAS4MessageForSendResult(
                        messageId: ctx.MessageEntityId.Value,
                        status: result);

                    await db.SaveChangesAsync()
                            .ConfigureAwait(false);
                }
            }

            if (ctx.AS4Message?.IsPullRequest == true)
            {
                using (DatastoreContext db = _createDatastore())
                {
                    var service = new PiggyBackingService(db);
                    service.ResetSignalMessagesToBePiggyBacked(ctx.AS4Message.SignalMessages, result);

                    await db.SaveChangesAsync()
                            .ConfigureAwait(false);
                }
            }
        }
    }
}