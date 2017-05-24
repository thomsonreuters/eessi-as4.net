using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
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

        private readonly ICertificateRepository _repository;
        private readonly IAS4ResponseHandler _responseHandler;
        private readonly Func<DatastoreContext> _createDatastore;
        private readonly IHttpClient _httpClient;

        private AS4Message _originalAS4Message;

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

            _responseHandler = new EmptyBodyResponseHandler(new PullRequestResponseHandler(new TailResponseHandler()));
            _repository = Registry.Instance.CertificateRepository;
        }

        /// <summary>
        /// Send the <see cref="AS4Message" />
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _originalAS4Message = internalMessage.AS4Message;

            return await TrySendAS4MessageAsync(internalMessage, cancellationToken).ConfigureAwait(false);
        }

        private async Task<StepResult> TrySendAS4MessageAsync(
            InternalMessage internalMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                HttpWebRequest request = CreateWebRequest(internalMessage);
                await TryHandleHttpRequestAsync(request, internalMessage, cancellationToken).ConfigureAwait(false);

                return await TryHandleHttpResponseAsync(request, internalMessage, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"{internalMessage.Prefix} An error occured while trying to send the message: {exception.Message}");

                return HandleSendAS4Exception(internalMessage, exception);
            }
        }

        protected virtual StepResult HandleSendAS4Exception(InternalMessage internalMessage, Exception exception)
        {
            if (internalMessage.SendingPMode.Reliability.ReceptionAwareness.IsEnabled)
            {
                // Set status to 'undetermined' and let ReceptionAwareness agent handle it.
                UpdateMessageStatus(_originalAS4Message, Operation.Undetermined, OutStatus.Exception);               
            }

            AS4Exception as4Exception = CreateFailedSendAS4Exception(internalMessage, exception);

            // Insert the exception in the OutException table.
            using (var context = _createDatastore())
            {
                var service = new OutExceptionService(context);
                service.InsertAS4Exception(as4Exception, _originalAS4Message);

                context.SaveChanges();
            }

            return StepResult.Failed(as4Exception, internalMessage).AndStopExecution();
        }

        private HttpWebRequest CreateWebRequest(InternalMessage message)
        {
            AS4Message as4Message = message.AS4Message;
            ISendConfiguration sendConfiguration = GetSendConfigurationFrom(message);

            HttpWebRequest request = _httpClient.Request(sendConfiguration.Protocol.Url, as4Message.ContentType);

            AssignClientCertificate(sendConfiguration.TlsConfiguration, request);

            return request;
        }

        private void AssignClientCertificate(TlsConfiguration configuration, HttpWebRequest request)
        {
            if (!configuration.IsEnabled || configuration.ClientCertificateReference == null)
            {
                return;
            }

            Logger.Info("Adding Client TLS Certificate to Http Request.");

            ClientCertificateReference certReference = configuration.ClientCertificateReference;
            X509Certificate2 certificate = _repository.GetCertificate(
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

        private async Task TryHandleHttpRequestAsync(
            HttpWebRequest request,
            InternalMessage internalMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                await SerializeHttpRequest(request, internalMessage, cancellationToken);
            }
            catch (WebException exception)
            {
                throw CreateFailedSendAS4Exception(internalMessage, exception);
            }
        }

        private static async Task SerializeHttpRequest(HttpWebRequest request, InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            AS4Message as4Message = internalMessage.AS4Message;

            Logger.Info($"Send AS4 Message to: {GetSendConfigurationFrom(internalMessage).Protocol.Url}");

            ISerializerProvider provider = Registry.Instance.SerializerProvider;
            long messageSize = as4Message.DetermineMessageSize(provider);
            const int threshold = 209_715_200;

            if (messageSize > threshold)
            {
                request.AllowWriteStreamBuffering = false;
                request.ContentLength = messageSize;
            }

            using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                ISerializer serializer = provider.Get(as4Message.ContentType);

                await serializer.SerializeAsync(as4Message, requestStream, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<StepResult> TryHandleHttpResponseAsync(
            HttpWebRequest request,
            InternalMessage internalMessage,
            CancellationToken cancellationToken)
        {
            Logger.Debug(
                $"AS4 Message received from: {GetSendConfigurationFrom(internalMessage).Protocol.Url}");

            // Since we've got here, the message has been sent.  Independently on the result whether it was correctly received or not, 
            // we've sent the message, so update the status to sent.
            UpdateMessageStatus(_originalAS4Message, Operation.Sent, OutStatus.Sent);

            (HttpWebResponse webResponse, WebException exception) response = await _httpClient.Respond(request);

            if (response.webResponse != null && ContentTypeSupporter.IsContentTypeSupported(response.webResponse.ContentType))
            {
                return await HandleAS4Response(internalMessage, response.webResponse, cancellationToken).ConfigureAwait(false);
            }

            throw CreateFailedSendAS4Exception(internalMessage, response.exception);
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

        private async Task<StepResult> HandleAS4Response(InternalMessage originalMessage, WebResponse webResponse, CancellationToken cancellation)
        {
            AS4Response as4Response = await AS4Response.Create(originalMessage, webResponse as HttpWebResponse, cancellation);
            return await _responseHandler.HandleResponse(as4Response).ConfigureAwait(false);
        }

        protected AS4Exception CreateFailedSendAS4Exception(InternalMessage internalMessage, Exception exception)
        {
            string protocolUrl = GetSendConfigurationFrom(internalMessage).Protocol.Url;
            string description = $"Failed to Send AS4 Message to Url: {protocolUrl}.";

            Logger.Error(description);
            Logger.Error(exception.Message);
            if (exception.InnerException != null)
            {
                Logger.Error(exception.InnerException.Message);
            }

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0005)
                .WithErrorAlias(ErrorAlias.ConnectionFailure)
                .WithMessageIds(internalMessage.AS4Message.MessageIds)
                .WithSendingPMode(internalMessage.SendingPMode)
                .WithInnerException(exception)
                .Build();
        }

        private static ISendConfiguration GetSendConfigurationFrom(InternalMessage message)
        {
            return message.AS4Message.IsPulling
                       ? (ISendConfiguration)message.SendingPMode?.PullConfiguration
                       : message.SendingPMode?.PushConfiguration;
        }
    }
}