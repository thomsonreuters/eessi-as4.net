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

        private MessagingContext _originalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class
        /// </summary>
        public SendAS4MessageStep() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class.
        /// Create a Send AS4Message Step
        /// with a given Serializer Provider
        /// </summary>
        /// <param name="createDatastore"></param>
        public SendAS4MessageStep(Func<DatastoreContext> createDatastore)
            : this(createDatastore, new ReliableHttpClient()) {}

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
            _originalMessage = messagingContext;

            try
            {
                HttpWebRequest request = CreateWebRequest(messagingContext);
                await TryHandleHttpRequestAsync(request, messagingContext).ConfigureAwait(false);

               return await TryHandleHttpResponseAsync(request, messagingContext, cancellation).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"{messagingContext.Prefix} An error occured while trying to send the message: {exception.Message}");

                return await HandleSendAS4ExceptionAsync(messagingContext, exception);
            }
        }

        private async Task<StepResult> HandleSendAS4ExceptionAsync(
            MessagingContext messagingContext,
            Exception exception)
        {
            if (messagingContext.SendingPMode.Reliability.ReceptionAwareness.IsEnabled)
            {
                // Set status to 'undetermined' and let ReceptionAwareness agent handle it.
                UpdateMessageStatus(_originalMessage.AS4Message, Operation.Undetermined, OutStatus.Exception);
            }

            messagingContext.ErrorResult = CreateConnectionFailureResult(exception, messagingContext);

            using (DatastoreContext context = _createDatastore())
            {
                var service = new OutExceptionService(context);
                await service.InsertError(messagingContext.ErrorResult, _originalMessage);

                context.SaveChanges();
            }

            return StepResult.Failed(messagingContext).AndStopExecution();
        }

        private HttpWebRequest CreateWebRequest(MessagingContext message)
        {
            AS4Message as4Message = message.AS4Message;
            ISendConfiguration sendConfiguration = GetSendConfigurationFrom(message);

            HttpWebRequest request = _httpClient.Request(sendConfiguration.Protocol.Url, as4Message.ContentType);

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

        private static async Task TryHandleHttpRequestAsync(HttpWebRequest request, MessagingContext messagingContext)
        {
            try
            {
                await SerializeHttpRequest(request, messagingContext);
            }
            catch (WebException exception)
            {
                throw CreateFailedSendAS4Exception(messagingContext, exception);
            }
        }

        private static async Task SerializeHttpRequest(HttpWebRequest request, MessagingContext messagingContext)
        {
            Logger.Info($"Send AS4 Message to: {GetSendConfigurationFrom(messagingContext).Protocol.Url}");

            long messageSize = TryGetMessageSize(messagingContext);
            const int threshold = 209_715_200;

            if (messageSize > threshold)
            {
                request.AllowWriteStreamBuffering = false;
                request.ContentLength = messageSize;
            }

            using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                await messagingContext.MessageStream.CopyToAsync(requestStream);
            }
        }

        private static long TryGetMessageSize(MessagingContext messagingContext)
        {
            try
            {
                return messagingContext.MessageStream.Length;
            }
            catch (Exception)
            {
                return 0L;
            }
        }

        private async Task<StepResult> TryHandleHttpResponseAsync(
            HttpWebRequest request,
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"AS4 Message received from: {GetSendConfigurationFrom(messagingContext).Protocol.Url}");

            // Since we've got here, the message has been sent.  Independently on the result whether it was correctly received or not, 
            // we've sent the message, so update the status to sent.
            UpdateMessageStatus(_originalMessage.AS4Message, Operation.Sent, OutStatus.Sent);

            (HttpWebResponse webResponse, WebException exception) response = await _httpClient.Respond(request);

            if (response.webResponse != null
                && ContentTypeSupporter.IsContentTypeSupported(response.webResponse.ContentType))
            {
                return
                    await HandleAS4Response(messagingContext, response.webResponse, cancellationToken)
                        .ConfigureAwait(false);
            }

            throw CreateFailedSendAS4Exception(messagingContext, response.exception);
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
            using (
                AS4Response as4Response = await AS4Response.Create(
                                              originalMessage,
                                              webResponse as HttpWebResponse,
                                              cancellation))
            {
                var responseHandler =
                    new EmptyBodyResponseHandler(new PullRequestResponseHandler(new TailResponseHandler()));
                return await responseHandler.HandleResponse(as4Response).ConfigureAwait(false);
            }
        }

        private static WebException CreateFailedSendAS4Exception(MessagingContext messagingContext, Exception exception)
        {
            string protocolUrl = GetSendConfigurationFrom(messagingContext).Protocol.Url;
            string description = $"Failed to Send AS4 Message to Url: {protocolUrl}.";

            return new WebException(description);
        }

        private static ErrorResult CreateConnectionFailureResult(Exception exception, MessagingContext context)
        {
            string protocolUrl = GetSendConfigurationFrom(context).Protocol.Url;
            string description = $"Failed to Send AS4 Message to Url: {protocolUrl}.";

            Logger.Error(description);
            Logger.Error(exception.Message);
            if (exception.InnerException != null)
            {
                Logger.Error(exception.InnerException.Message);
            }

            return new ErrorResult(description, ErrorCode.Ebms0005, ErrorAlias.ConnectionFailure);
        }

        private static ISendConfiguration GetSendConfigurationFrom(MessagingContext message)
        {
            return message.AS4Message.IsPullRequest
                       ? (ISendConfiguration) message.SendingPMode?.PullConfiguration
                       : message.SendingPMode?.PushConfiguration;
        }
    }
}