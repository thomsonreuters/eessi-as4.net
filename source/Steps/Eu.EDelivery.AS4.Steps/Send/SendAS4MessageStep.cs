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
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Send <see cref="AS4Message" /> to the corresponding Receiving MSH
    /// </summary>
    public class SendAS4MessageStep : IStep
    {
        private readonly ILogger _logger;
        private readonly ISerializerProvider _provider;
        private readonly ICertificateRepository _repository;

        private AS4Message _originalAS4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep"/> class
        /// </summary>
        public SendAS4MessageStep()
        {
            this._provider = Registry.Instance.SerializerProvider;
            this._repository = Registry.Instance.CertificateRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep"/> class. 
        /// Create a Send AS4Message Step 
        /// with a given Serializer Provider
        /// </summary>
        /// <param name="provider">
        /// </param>
        public SendAS4MessageStep(ISerializerProvider provider)
        {
            this._provider = provider;
            this._logger = LogManager.GetCurrentClassLogger();
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

            return await TrySendAS4MessageAsync(internalMessage, cancellationToken);
        }

        private async Task<StepResult> TrySendAS4MessageAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                HttpWebRequest request = CreateWebRequest(internalMessage.AS4Message);
                await TryHandleHttpRequestAsync(request, internalMessage, cancellationToken);
                return await TryHandleHttpResponseAsync(request, internalMessage, cancellationToken);
            }
            catch (Exception exception)
            {
                this._logger.Error(
                    $"{internalMessage.Prefix} An error occured while trying to send the message: {exception.Message}");

                return HandleSendAS4Exception(internalMessage, exception);
            }
        }

        protected virtual StepResult HandleSendAS4Exception(InternalMessage internalMessage, Exception exception)
        {
            if (internalMessage.AS4Message.SendingPMode.Reliability.ReceptionAwareness.IsEnabled)
            {
                // Set status to 'undetermined' and let ReceptionAwareness agent handle it.
                UpdateOperation(_originalAS4Message, Operation.Undetermined);

                return StepResult.Failed(AS4ExceptionBuilder.WithDescription("Failed to send AS4Message").WithInnerException(exception).Build());
            }
            else
            {
                var as4Exception = CreateFailedSendAS4Exception(internalMessage, exception);
                internalMessage.AS4Message?.SignalMessages?.Clear();
                return StepResult.Failed(as4Exception, internalMessage);
            }
        }

        private static void UpdateOperation(AS4Message as4Message, Operation operation)
        {            
            if (as4Message != null)
            {
                using (var context = Registry.Instance.CreateDatastoreContext())
                {
                    var repository = new DatastoreRepository(context);

                    var outMessages = repository.GetOutMessagesById(as4Message.MessageIds);

                    foreach (var outMessage in outMessages)
                    {
                        outMessage.Operation = operation;
                    }

                    context.SaveChanges();
                }
            }
        }

        private HttpWebRequest CreateWebRequest(AS4Message as4Message)
        {
            string protocolUrl = as4Message.SendingPMode.PushConfiguration.Protocol.Url;

            var request = (HttpWebRequest)WebRequest.Create(protocolUrl);
            request.Method = "POST";
            request.ContentType = as4Message.ContentType;
            request.KeepAlive = false;
            request.Connection = "Open";
            request.ProtocolVersion = HttpVersion.Version11;

            AssignClientCertificate(as4Message.SendingPMode.PushConfiguration.TlsConfiguration, request);
            ServicePointManager.Expect100Continue = false;

            return request;
        }

        private void AssignClientCertificate(TlsConfiguration configuration, HttpWebRequest request)
        {
            if (!configuration.IsEnabled || configuration.ClientCertificateReference == null)
            {
                return;
            }

            _logger.Info("Adding Client TLS Certificate to Http Request.");

            ClientCertificateReference certReference = configuration.ClientCertificateReference;
            X509Certificate2 certificate = this._repository
                .GetCertificate(certReference.ClientCertificateFindType, certReference.ClientCertificateFindValue);

            if (certificate == null)
            {
                throw new ConfigurationErrorsException($"The Client TLS Certificate could not be found (FindType:{certReference.ClientCertificateFindType}/FindValue:{certReference.ClientCertificateFindValue})");
            }
            else
            {
                request.ClientCertificates.Add(certificate);
            }
        }

        private async Task TryHandleHttpRequestAsync(WebRequest request, InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                this._logger.Info($"Send AS4 Message to: {internalMessage.AS4Message.SendingPMode.PushConfiguration.Protocol.Url}");
                await HandleHttpRequestAsync(request, internalMessage.AS4Message, cancellationToken);
            }
            catch (WebException exception)
            {
                throw CreateFailedSendAS4Exception(internalMessage, exception);
            }
        }

        private async Task HandleHttpRequestAsync(WebRequest request, AS4Message as4Message, CancellationToken cancellationToken)
        {
            ISerializer serializer = this._provider.Get(as4Message.ContentType);

            using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                serializer.Serialize(as4Message, requestStream, cancellationToken);
            }
        }

        private async Task<StepResult> TryHandleHttpResponseAsync(WebRequest request, InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Debug($"AS4 Message received from: {internalMessage.AS4Message.SendingPMode.PushConfiguration.Protocol.Url}");
                return await HandleHttpResponseAsync(request, internalMessage, cancellationToken);
            }
            catch (WebException exception)
            {                                
                if (exception.Response != null &&
                    ContentTypeSupporter.IsContentTypeSupported(exception.Response.ContentType))
                {
                    return await PrepareStepResult(exception.Response as HttpWebResponse, internalMessage, cancellationToken);
                }
                
                throw CreateFailedSendAS4Exception(internalMessage, exception);
            }
        }

        private async Task<StepResult> HandleHttpResponseAsync(WebRequest request, InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            // Since we've got here, the message has been sent.  Independently on the result whether it was correctly received or not, 
            // we've sent the message, so update the status to sent.
            UpdateOperation(_originalAS4Message, Operation.Sent);

            using (WebResponse responseStream = await request.GetResponseAsync().ConfigureAwait(false))
            {                
                return await PrepareStepResult(responseStream as HttpWebResponse, internalMessage, cancellationToken);
            }
        }

        private async Task<StepResult> PrepareStepResult(HttpWebResponse webResponse, InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            internalMessage.AS4Message = this._originalAS4Message;

            if (webResponse == null || webResponse.StatusCode == HttpStatusCode.Accepted)
            {
                internalMessage.AS4Message.SignalMessages.Clear();

                this._logger.Info("Empty Soap Body is returned, no further action is needed");
                return await StepResult.SuccessAsync(internalMessage);
            }

            var response = await DeserializeHttpResponse(webResponse, cancellationToken);
            response.SendingPMode = internalMessage.AS4Message.SendingPMode;
            response.UserMessages.Add(internalMessage.AS4Message.PrimaryUserMessage);

            internalMessage.AS4Message = response;

            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task<AS4Message> DeserializeHttpResponse(HttpWebResponse webResponse, CancellationToken cancellationToken)
        {
            string contentType = webResponse.ContentType;
            Stream responseStream = webResponse.GetResponseStream();

            ISerializer serializer = this._provider.Get(contentType: contentType);
            return await serializer.DeserializeAsync(responseStream, contentType, cancellationToken);
        }

        protected AS4Exception CreateFailedSendAS4Exception(InternalMessage internalMessage, Exception exception)
        {
            string protocolUrl = internalMessage.AS4Message.SendingPMode.PushConfiguration.Protocol.Url;
            string description = $"Failed to Send AS4 Message to Url: {protocolUrl}.";
            this._logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0005)
                .WithExceptionType(ExceptionType.ConnectionFailure)
                .WithMessageIds(internalMessage.AS4Message.MessageIds)
                .WithSendingPMode(internalMessage.AS4Message.SendingPMode)
                .WithInnerException(exception)
                .Build();
        }
    }
}