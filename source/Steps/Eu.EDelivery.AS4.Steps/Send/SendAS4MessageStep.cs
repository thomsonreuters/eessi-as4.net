using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Send <see cref="AS4Message" /> to the corresponding Receiving MSH
    /// </summary>
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
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellation)
        {
            if (messagingContext.ReceivedMessage == null)
            {
                throw new InvalidOperationException("SendAS4MessageStep requires a MessagingContext with a ReceivedStream");
            }

            // TODO: modify this; PullConfig and PushConfig: no distinction.
            var sendConfiguration = messagingContext.SendingPMode.PushConfiguration; // GetSendConfigurationFrom(as4MessageHeader.IsPullRequest, messagingContext.SendingPMode);
            AS4Message as4Message = AS4Message.Empty;
            
            try
            {
                using (messagingContext)
                {
                    HttpWebRequest request = CreateWebRequest(sendConfiguration, messagingContext.ReceivedMessage.ContentType);

                    as4Message = await TryWriteToHttpRequestStreamAsync(request, messagingContext).ConfigureAwait(false);

                    var sentContext = messagingContext.CloneWith(as4Message);

                    return await TryHandleHttpResponseAsync(request, sentContext, cancellation).ConfigureAwait(false);
                }

            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"{messagingContext.Prefix} An error occured while trying to send the message: {exception.Message}");

                var errorResult = await HandleSendExceptionAsync(sendConfiguration.Protocol.Url, as4Message, messagingContext.SendingPMode, exception);
                messagingContext.ErrorResult = errorResult;

                return StepResult.Failed(messagingContext);
            }
        }

        private async Task<ErrorResult> HandleSendExceptionAsync(
            string requestUrl,
            AS4Message as4MessageHeader,
            SendingProcessingMode sendingPMode,
            Exception exception)
        {
            if (sendingPMode.Reliability.ReceptionAwareness.IsEnabled)
            {
                // Set status to 'undetermined' and let ReceptionAwareness agent handle it.
                UpdateMessageStatus(as4MessageHeader, Operation.Undetermined, OutStatus.Exception);
            }

            var errorResult = CreateConnectionFailureResult(requestUrl, exception);

            using (DatastoreContext context = _createDatastore())
            {
                var service = new OutExceptionService(context);
                await service.InsertException(errorResult, as4MessageHeader, sendingPMode);

                context.SaveChanges();
            }

            return errorResult;
        }

        private HttpWebRequest CreateWebRequest(ISendConfiguration sendConfiguration, string contentType)
        {
            HttpWebRequest request = _httpClient.Request(sendConfiguration.Protocol.Url, contentType);

            AssignClientCertificate(sendConfiguration.TlsConfiguration, request);

            return request;
        }

        private static void AssignClientCertificate(TlsConfiguration configuration, HttpWebRequest request)
        {
            if (!configuration.IsEnabled || configuration.ClientCertificateReference == null)
            {
                return;
            }

            Logger.Info("Adding Client TLS Certificate to Http Request.");

            ClientCertificateReference certReference = configuration.ClientCertificateReference;
            X509Certificate2 certificate =
                Registry.Instance.CertificateRepository.GetCertificate(
                    certReference.ClientCertificateFindType,
                    certReference.ClientCertificateFindValue);

            if (certificate == null)
            {
                throw new ConfigurationErrorsException(
                    "The Client TLS Certificate could not be found "
                    + $"(FindType:{certReference.ClientCertificateFindType}/FindValue:{certReference.ClientCertificateFindValue})");
            }

            request.ClientCertificates.Add(certificate);
        }

        private static async Task<AS4Message> TryWriteToHttpRequestStreamAsync(HttpWebRequest request, MessagingContext messagingContext)
        {
            try
            {
                await SerializeHttpRequest(request, messagingContext);

                // Get the AS4 Message representation of the stream that has been written to HTTP
                var sentStream = messagingContext.ReceivedMessage;
                var serializer = SerializerProvider.Default.Get(sentStream.ContentType);

                sentStream.UnderlyingStream.Position = 0;
                return await serializer.DeserializeAsync(sentStream.UnderlyingStream, sentStream.ContentType,
                                                   CancellationToken.None);
            }
            catch (WebException exception)
            {
                throw CreateFailedSendException(request.RequestUri.ToString(), exception);
            }
        }

        private static async Task SerializeHttpRequest(HttpWebRequest request, MessagingContext messagingContext)
        {
            Logger.Info($"Send AS4 Message to: {request.RequestUri}");

            long messageSize = TryGetMessageSize(messagingContext);
            const int threshold = 209_715_200;

            if (messageSize > threshold)
            {
                request.AllowWriteStreamBuffering = false;
                request.ContentLength = messageSize;
            }

            using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await messagingContext.ReceivedMessage.UnderlyingStream.CopyToAsync(requestStream);
            }
        }

        private static long TryGetMessageSize(MessagingContext messagingContext)
        {
            if (messagingContext.ReceivedMessage.UnderlyingStream.CanSeek)
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
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"AS4 Message received from: {request.Address}");

            // Since we've got here, the message has been sent.  Independently on the result whether it was correctly received or not, 
            // we've sent the message, so update the status to sent.
            UpdateMessageStatus(messagingContext.AS4Message, Operation.Sent, OutStatus.Sent);

            (HttpWebResponse webResponse, WebException exception) response = await _httpClient.Respond(request);

            if (response.webResponse != null
                && ContentTypeSupporter.IsContentTypeSupported(response.webResponse.ContentType))
            {
                return
                    await HandleAS4Response(messagingContext, response.webResponse, cancellationToken)
                        .ConfigureAwait(false);
            }

            throw CreateFailedSendException(request.RequestUri.ToString(), response.exception);
        }

        private void UpdateMessageStatus(AS4Message as4Message, Operation operation, OutStatus status)
        {
            if (as4Message == null)
            {
                return;
            }

            using (DatastoreContext context = _createDatastore())
            {
                var repository = new DatastoreRepository(context);

                repository.UpdateOutMessages(
                    as4Message.MessageIds,
                    outMessage =>
                    {
                        outMessage.Operation = operation;
                        outMessage.Status = status;
                    });

                context.SaveChanges();
            }
        }

        private static async Task<StepResult> HandleAS4Response(
            MessagingContext originalMessage,
            WebResponse webResponse,
            CancellationToken cancellation)
        {
            using (AS4Response as4Response =
                await AS4Response.Create(originalMessage, webResponse as HttpWebResponse, cancellation))
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

        private static ErrorResult CreateConnectionFailureResult(string requestUrl, Exception exception)
        {

            string description = $"Failed to Send AS4 Message to Url: {requestUrl}.";

            Logger.Error(description);
            Logger.Error(exception.Message);
            if (exception.InnerException != null)
            {
                Logger.Error(exception.InnerException.Message);
            }

            return new ErrorResult(description, ErrorCode.Ebms0005, ErrorAlias.ConnectionFailure);
        }
    }
}