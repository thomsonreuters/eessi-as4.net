using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Send.Response;
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
        public SendAS4MessageStep() :
            this(Registry.Instance.CreateDatastoreContext, new ReliableHttpClient())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class.
        /// </summary>
        /// <param name="createDatastore">Delegate to create a new context.</param>
        /// <param name="client">Instance to handle the HTTP response.</param>
        public SendAS4MessageStep(Func<DatastoreContext> createDatastore, IHttpClient client)
        {
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

            SendingProcessingMode sendPMode = messagingContext.SendingPMode;
            PushConfiguration sendConfiguration = sendPMode?.PushConfiguration;

            if (sendConfiguration == null)
            {
                throw new ConfigurationErrorsException(
                    $"{messagingContext.LogTag} Message cannot be send: " +
                    $"SendingPMode {sendPMode?.Id} does not contain a <PushConfiguration/> element");
            }

            AS4Message as4Message = await DeserializeUnderlyingStreamIfPresent(
                messagingContext.ReceivedMessage,
                otherwise: messagingContext.AS4Message);

            return await SendAS4MessageAsync(messagingContext, sendPMode, as4Message);
        }

        private static async Task<AS4Message> DeserializeUnderlyingStreamIfPresent(ReceivedMessage rm, AS4Message otherwise)
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
                              rm.ContentType,
                              CancellationToken.None)
                        .ConfigureAwait(false);

                // TODO: the serializer already does this?
                rm.UnderlyingStream.Position = 0;

                return as4Message;
            }

            return otherwise;
        }

        private async Task<StepResult> SendAS4MessageAsync(
            MessagingContext ctx,
            SendingProcessingMode sendPMode,
            AS4Message as4Message)
        {
            try
            {
                string contentType = ctx.ReceivedMessage?.ContentType ?? ctx.AS4Message.ContentType;
                HttpWebRequest request = CreateWebRequest(sendPMode, contentType.Replace("charset=\"utf-8\"", ""));

                if (await WriteToHttpRequestStreamAsync(request, ctx).ConfigureAwait(false))
                {
                    ctx.ModifyContext(as4Message);

                    return await TryHandleHttpResponseAsync(request, ctx).ConfigureAwait(false);
                }

                return StepResult.Failed(ctx);
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"{ctx.LogTag} An error occured while trying to send the message: {exception}");

                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }

                throw;
            }
            finally
            {
                await UpdateMessageStatusAsync(ctx, Operation.Sent, OutStatus.Sent).ConfigureAwait(false);
            }
        }

        private HttpWebRequest CreateWebRequest(
            SendingProcessingMode sendPMode,
            string contentType)
        {
            var url = sendPMode.PushConfiguration.Protocol.Url;
            Logger.Debug($"Creating WebRequest to {url}");

            HttpWebRequest request = _httpClient.Request(url, contentType);

            X509Certificate2 clientCert = RetrieveClientCertificate(sendPMode);
            if (clientCert != null)
            {
                request.ClientCertificates.Add(clientCert);
            }

            return request;
        }

        private static X509Certificate2 RetrieveClientCertificate(SendingProcessingMode sendPMode)
        {
            TlsConfiguration tlsConfig = sendPMode.PushConfiguration.TlsConfiguration;
            if (!tlsConfig.IsEnabled || tlsConfig.ClientCertificateInformation == null)
            {
                return null;
            }

            Logger.Trace("Adding Client TLS Certificate to Http Request");

            X509Certificate2 certificate = RetrieveTlsCertificate(tlsConfig);

            if (certificate == null)
            {
                throw new NotSupportedException(
                    "The TLS certificate information specified in the Sending PMode " +
                    $"{sendPMode.Id} could not be used to retrieve the certificate");
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

        private static async Task<bool> WriteToHttpRequestStreamAsync(HttpWebRequest request, MessagingContext ctx)
        {
            try
            {
                SetAdditionalRequestHeaders(request, ctx);

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
                        SerializerProvider
                            .Default
                            .Get(request.ContentType)
                            .Serialize(ctx.AS4Message, requestStream, CancellationToken.None);
                    }
                }

                return true;
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure
                    && (ctx.AS4Message?.IsPullRequest ?? false))
                {
                    Logger.Trace($"The PullRequest could not be send to {request.RequestUri} due to a WebException");
                    Logger.Trace(exception.Message);
                    return false;
                }

                Logger.Error(exception.Message);
                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }

                throw CreateFailedSendException(request.RequestUri.ToString(), exception);
            }
        }

        private static void SetAdditionalRequestHeaders(HttpWebRequest request, MessagingContext messagingContext)
        {
            long messageSize = TryGetMessageSize(messagingContext);
            if (messageSize >= 0)
            {
                request.ContentLength = messageSize;
            }

            request.AllowWriteStreamBuffering = false;
        }

        private static long TryGetMessageSize(MessagingContext ctx)
        {
            if (ctx.ReceivedMessage?.UnderlyingStream?.CanSeek ?? false)
            {
                return ctx.ReceivedMessage.UnderlyingStream.Length;
            }

            return ctx.AS4Message?.DetermineMessageSize(SerializerProvider.Default) ?? 0L;
        }

        private async Task<StepResult> TryHandleHttpResponseAsync(
            HttpWebRequest request,
            MessagingContext ctx)
        {
            Logger.Debug($"AS4Message received from: {request.Address}");

            (HttpWebResponse webResponse, WebException exception) =
                await _httpClient.Respond(request).ConfigureAwait(false);

            if (webResponse != null
                && ContentTypeSupporter.IsContentTypeSupported(webResponse.ContentType))
            {
                return
                    await HandleAS4Response(ctx, webResponse)
                        .ConfigureAwait(false);
            }

            throw CreateFailedSendException(request.RequestUri.ToString(), exception);
        }

        private async Task UpdateMessageStatusAsync(MessagingContext ctx, Operation operation, OutStatus status)
        {
            if (ctx.MessageEntityId == null)
            {
                return;
            }

            using (DatastoreContext db = _createDatastore())
            {
                var repository = new DatastoreRepository(db);

                repository.UpdateOutMessage(
                    ctx.MessageEntityId.Value,
                    updateAction: outMessage =>
                    {
                        outMessage.Operation = operation;
                        outMessage.SetStatus(status);
                    });

                Entities.ReceptionAwareness ra =
                    repository.GetReceptionAwarenessForOutMessage(ctx.MessageEntityId.Value);

                if (ra != null)
                {
                    ra.LastSendTime = DateTimeOffset.Now;
                    ra.CurrentRetryCount += 1;
                }

                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static async Task<StepResult> HandleAS4Response(
            MessagingContext originalMessage,
            WebResponse webResponse)
        {
            using (AS4Response res =
                await AS4Response.Create(
                    requestMessage: originalMessage,
                    webResponse: webResponse as HttpWebResponse).ConfigureAwait(false))
            {
                var handler = new EmptyBodyResponseHandler(
                    new PullRequestResponseHandler(
                        new TailResponseHandler()));

                return await handler
                    .HandleResponse(res)
                    .ConfigureAwait(false);
            }
        }

        private static WebException CreateFailedSendException(string requestUrl, Exception exception)
        {
            return new WebException($"Failed to Send AS4Message to Url: {requestUrl}.", exception);
        }
    }
}