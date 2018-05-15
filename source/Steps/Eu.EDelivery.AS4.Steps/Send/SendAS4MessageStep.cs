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
        public SendAS4MessageStep() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class.
        /// Create a Send AS4Message Step
        /// with a given Serializer Provider
        /// </summary>
        /// <param name="createDatastore"></param>
        public SendAS4MessageStep(Func<DatastoreContext> createDatastore)
            : this(createDatastore, new ReliableHttpClient()) { }

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
                    $"{messagingContext.LogTag} {nameof(SendAS4MessageStep)} " + 
                    "requires a MessagingContext with a ReceivedStream or an AS4 Message to correctly send the message");
            }

            if (messagingContext.ReceivedMessage == null && messagingContext.AS4Message.IsPullRequest == false)
            {
                throw new InvalidOperationException(
                    $"{messagingContext.LogTag} {nameof(SendAS4MessageStep)} " + 
                    "expects a PullRequest AS4 Message when the MessagingContext does not contain a ReceivedStream");
            }

            PushConfiguration sendConfiguration = messagingContext.SendingPMode.PushConfiguration;

            if (sendConfiguration == null)
            {
                throw new ConfigurationErrorsException(
                    $"{messagingContext.LogTag} Message cannot be send: "+ 
                    $"SendingPMode {messagingContext.SendingPMode.Id} does not contain a <PushConfiguration/> element");
            }

            AS4Message as4Message = await GetAS4MessageFromContextAsync(messagingContext);

            try
            {
                string contentType = messagingContext.ReceivedMessage?.ContentType ?? messagingContext.AS4Message.ContentType;

                contentType = contentType.Replace("charset=\"utf-8\"", "");

                HttpWebRequest request = CreateWebRequest(messagingContext.SendingPMode, contentType);

                if (await TryWriteToHttpRequestStreamAsync(request, messagingContext).ConfigureAwait(false))
                {
                    messagingContext.ModifyContext(as4Message);

                    return await TryHandleHttpResponseAsync(request, messagingContext).ConfigureAwait(false);
                }

                return StepResult.Failed(messagingContext);
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"{messagingContext.LogTag} An error occured while trying to send the message: {exception}");

                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }

                throw;
            }
            finally
            {
                await UpdateMessageStatusAsync(messagingContext, Operation.Sent, OutStatus.Sent).ConfigureAwait(false);
            }
        }

        private HttpWebRequest CreateWebRequest(
            SendingProcessingMode pmode, 
            string contentType)
        {
            var url = pmode.PushConfiguration.Protocol.Url;
            Logger.Debug($"Creating WebRequest to {url}");

            HttpWebRequest request = _httpClient.Request(url, contentType);

            X509Certificate2 clientCert = RetrieveClientCertificate(pmode);
            if (clientCert != null)
            {
                request.ClientCertificates.Add(clientCert); 
            }

            return request;
        }

        private static X509Certificate2 RetrieveClientCertificate(SendingProcessingMode pmode)
        {
            var configuration = pmode.PushConfiguration.TlsConfiguration;
            if (!configuration.IsEnabled || configuration.ClientCertificateInformation == null)
            {
                return null;
            }

            Logger.Trace("Adding Client TLS Certificate to Http Request");

            X509Certificate2 certificate = RetrieveTlsCertificate(configuration);

            if (certificate == null)
            {
                throw new NotSupportedException(
                    $"The TLS certificate information specified in the Sending PMode {pmode.Id} could not be used to retrieve the certificate");
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

        private static async Task<bool> TryWriteToHttpRequestStreamAsync(HttpWebRequest request, MessagingContext messagingContext)
        {
            try
            {
                SetAdditionalRequestHeaders(request, messagingContext);

                using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    if (messagingContext.ReceivedMessage != null)
                    {
                        await messagingContext.ReceivedMessage.UnderlyingStream.CopyToFastAsync(requestStream).ConfigureAwait(false);
                    }
                    else
                    {
                        // Serialize the AS4 Message to the request-stream
                        var serializer = SerializerProvider.Default.Get(request.ContentType);
                        serializer.Serialize(messagingContext.AS4Message, requestStream, CancellationToken.None);
                    }
                }

                return true;
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure && (messagingContext.AS4Message?.IsPullRequest ?? false))
                {
                    Logger.Trace($"{messagingContext.LogTag}The PullRequest could not be send to {request.RequestUri} due to a WebException");
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

        private static async Task<AS4Message> GetAS4MessageFromContextAsync(MessagingContext context)
        {
            if (context.ReceivedMessage != null)
            {
                return await GetAS4MessageFromStreamAsync(context.ReceivedMessage.UnderlyingStream, context.ReceivedMessage.ContentType).ConfigureAwait(false);
            }
            else
            {
                return context.AS4Message;
            }
        }

        private static async Task<AS4Message> GetAS4MessageFromStreamAsync(Stream stream, string contentType)
        {
            var serializer = SerializerProvider.Default.Get(contentType);

            stream.Position = 0;
            var as4Message = await serializer.DeserializeAsync(stream, contentType, CancellationToken.None).ConfigureAwait(false);
            stream.Position = 0;

            return as4Message;
        }

        private static long TryGetMessageSize(MessagingContext messagingContext)
        {
            if (messagingContext.ReceivedMessage?.UnderlyingStream?.CanSeek ?? false)
            {
                return messagingContext.ReceivedMessage.UnderlyingStream.Length;
            }

            if (messagingContext.AS4Message != null)
            {
                return messagingContext.AS4Message.DetermineMessageSize(SerializerProvider.Default);
            }

            return 0L;
        }

        private async Task<StepResult> TryHandleHttpResponseAsync(
            HttpWebRequest request,
            MessagingContext messagingContext)
        {
            Logger.Debug($"{messagingContext.LogTag} AS4 Message received from: {request.Address}");

            (HttpWebResponse webResponse, WebException exception) response = await _httpClient.Respond(request).ConfigureAwait(false);

            if (response.webResponse != null
                && ContentTypeSupporter.IsContentTypeSupported(response.webResponse.ContentType))
            {
                return
                    await HandleAS4Response(messagingContext, response.webResponse)
                        .ConfigureAwait(false);
            }

            throw CreateFailedSendException(request.RequestUri.ToString(), response.exception);
        }

        private async Task UpdateMessageStatusAsync(MessagingContext messagingContext, Operation operation, OutStatus status)
        {
            if (messagingContext.MessageEntityId == null)
            {
                return;
            }

            using (DatastoreContext context = _createDatastore())
            {
                var repository = new DatastoreRepository(context);

                repository.UpdateOutMessage(
                    messagingContext.MessageEntityId.Value,
                    updateAction: outMessage =>
                    {
                        outMessage.SetOperation(operation);
                        outMessage.SetStatus(status);
                    });

                var receptionAwareness =
                    repository.GetReceptionAwarenessForOutMessage(messagingContext.MessageEntityId.Value);

                if (receptionAwareness != null)
                {
                    receptionAwareness.LastSendTime = DateTimeOffset.Now;
                    receptionAwareness.CurrentRetryCount += 1;
                }

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static async Task<StepResult> HandleAS4Response(
            MessagingContext originalMessage,
            WebResponse webResponse)
        {
            using (AS4Response as4Response =
                await AS4Response.Create(originalMessage, webResponse as HttpWebResponse).ConfigureAwait(false))
            {
                var responseHandler = new EmptyBodyResponseHandler(new PullRequestResponseHandler(new TailResponseHandler()));
                return await responseHandler.HandleResponse(as4Response).ConfigureAwait(false);
            }
        }

        private static WebException CreateFailedSendException(string requestUrl, Exception exception)
        {
            string description = $"Failed to Send AS4 Message to Url: {requestUrl}.";

            return new WebException(description, exception);
        }
    }
}