using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using RetryReliability = Eu.EDelivery.AS4.Model.PMode.RetryReliability;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Service model to abstract away the insertion of <see cref="InException"/> and <see cref="OutException"/>s 
    /// based on the wheter we have to insert an exception during the Submit-, Transformation- or other processing operations for an <see cref="AS4Message"/>.
    /// </summary>
    internal class ExceptionService
    {
        private readonly IConfig _config;
        private readonly IDatastoreRepository _repository;
        private readonly IAS4MessageBodyStore _bodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionService"/> class.
        /// </summary>
        public ExceptionService(
            IConfig config,
            IDatastoreRepository repository,
            IAS4MessageBodyStore bodyStore)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (bodyStore == null)
            {
                throw new ArgumentNullException(nameof(bodyStore));
            }

            _config = config;
            _repository = repository;
            _bodyStore = bodyStore;
        }

        /// <summary>
        /// Insert an <see cref="InException"/> based on an exception that occurred dring the incoming Submit operation.
        /// </summary>
        /// <param name="exception">The exception which message will be inserted.</param>
        /// <param name="submit">The original message that caused the exception.</param>
        /// <param name="pmode">The PMode that was being used during the Submit operation.</param>
        public async Task<InException> InsertIncomingSubmitExceptionAsync(
            Exception exception, 
            SubmitMessage submit, 
            ReceivingProcessingMode pmode)
        {
            Stream stream = await AS4XmlSerializer.ToStreamAsync(submit);
            string location = await _bodyStore.SaveAS4MessageStreamAsync(
                _config.InExceptionStoreLocation,
                stream);

            InException entity = 
                InException
                    .ForMessageBody(messageLocation: location, exception: exception)
                    .SetOperationFor(pmode?.ExceptionHandling);

            await entity.SetPModeInformationAsync(pmode);

            _repository.InsertInException(entity);

            return entity;
        }

        /// <summary>
        /// Insert an <see cref="InException"/> based on an exception that occured during the incoming processing of an <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="exception">The exception which message will be inserted.</param>
        /// <param name="ebmsMessageId">The primary message id of the <see cref="AS4Message"/> that caused the exception.</param>
        /// <param name="pmode">The PMode that was being used during the processing.</param>
        public async Task<InException> InsertIncomingAS4MessageExceptionAsync(Exception exception, string ebmsMessageId, ReceivingProcessingMode pmode)
        {
            InException entity = 
                InException
                    .ForEbmsMessageId(ebmsMessageId, exception)
                    .SetOperationFor(pmode?.ExceptionHandling);

            await entity.SetPModeInformationAsync(pmode);

            _repository.InsertInException(entity);
            _repository.UpdateInMessage(ebmsMessageId, m => m.SetStatus(InStatus.Exception));

            return entity;
        }

        /// <summary>
        /// Insert a <see cref="RetryReliability"/> record for an stored <see cref="InException"/> record.
        /// </summary>
        /// <param name="referenced">The referenced exception record.</param>
        /// <param name="reliability">Reliability to populate the record with retry information.</param>
        public void InsertRelatedRetryReliability(InException referenced, RetryReliability reliability)
        {
            if (referenced == null)
            {
                throw new ArgumentNullException(nameof(referenced));
            }

            if (referenced.Id <= 0)
            {
                throw new InvalidOperationException(
                    "Requires to have a stored InException to insert a referenced RetryReliability record");
            }

            if (reliability != null && reliability.IsEnabled)
            {
                var r = Entities.RetryReliability.CreateForInException(
                    refToInExceptionId: referenced.Id,
                    maxRetryCount: reliability.RetryCount,
                    retryInterval: reliability.RetryInterval.AsTimeSpan(),
                    type: RetryType.Notification);

                _repository.InsertRetryReliability(r);
            }
        }

        /// <summary>
        /// Insert an <see cref="OutException"/> based on an exception that occured during the outgoing Submit operation.
        /// </summary>
        /// <param name="exception">The exception which message will be inserted.</param>
        /// <param name="submit">The message that caused the exception.</param>
        /// <param name="pmode">The PMode that was being used during the Submit operation.</param>
        public async Task<OutException> InsertOutgoingSubmitExceptionAsync(
            Exception exception,
            SubmitMessage submit,
            SendingProcessingMode pmode)
        {
            Stream stream = await AS4XmlSerializer.ToStreamAsync(submit);
            string location = await _bodyStore.SaveAS4MessageStreamAsync(
                _config.OutExceptionStoreLocation,
                stream);

            OutException entity = 
                 OutException
                     .ForMessageBody(location, exception)
                     .SetOperationFor(pmode?.ExceptionHandling);

            await entity.SetPModeInformationAsync(pmode);

            _repository.InsertOutException(entity);

            return entity;
        }

        /// <summary>
        /// Insert an <see cref="OutException"/> based on an exception that occured during the outgoing processing of an <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="exception">The exception which message will be inserted.</param>
        /// <param name="ebmsMessageId">The primary message id of the <see cref="AS4Message"/> that caused the exception</param>
        /// <param name="entityId">The primary key of the stored record to which the mesage is refering to.</param>
        /// <param name="pmode">The PMode that was used during the processing of the message.</param>
        public async Task<OutException> InsertOutgoingAS4MessageExceptionAsync(
            Exception exception,
            string ebmsMessageId,
            long? entityId,
            SendingProcessingMode pmode)
        {
            OutException entity = 
                OutException
                    .ForEbmsMessageId(ebmsMessageId, exception)
                    .SetOperationFor(pmode?.ExceptionHandling);

            await entity.SetPModeInformationAsync(pmode);

            _repository.InsertOutException(entity);

            if (entityId.HasValue)
            {
                _repository.UpdateOutMessage(entityId.Value, m => m.SetStatus(OutStatus.Exception));
            }

            return entity;
        }

        /// <summary>
        /// Insert a <see cref="RetryReliability"/> record for an stored <see cref="OutException"/> record.
        /// </summary>
        /// <param name="referenced">The referenced exception record.</param>
        /// <param name="reliability">Reliability to populate the record with retry information.</param>
        public void InsertRelatedRetryReliability(OutException referenced, RetryReliability reliability)
        {
            if (referenced == null)
            {
                throw new ArgumentNullException(nameof(referenced));
            }

            if (referenced.Id <= 0)
            {
                throw new InvalidOperationException(
                    "Requires to have a stored OutException to insert a referenced RetryReliability record");
            }

            if (reliability != null && reliability.IsEnabled)
            {
                var r = Entities.RetryReliability.CreateForOutException(
                    refToOutExceptionId: referenced.Id,
                    maxRetryCount: reliability.RetryCount,
                    retryInterval: reliability.RetryInterval.AsTimeSpan(),
                    type: RetryType.Notification);

                _repository.InsertRetryReliability(r);
            }
        }

        /// <summary>
        /// Insert an <see cref="InException"/> based on an exception that occured during an incoming operation for which we do not have a valid/complete message.
        /// Example: transformation.
        /// </summary>
        /// <param name="exception">The exception which message will be inserted.</param>
        /// <param name="messageStream">The stream that represents the message which caused the exception.</param>
        public async Task InsertIncomingExceptionAsync(Exception exception, Stream messageStream)
        {
            string location = await _bodyStore.SaveAS4MessageStreamAsync(
                _config.InExceptionStoreLocation,
                messageStream);

            InException entity = InException.ForMessageBody(location, exception);
            _repository.InsertInException(entity);
        }

        /// <summary>
        /// Insert an <see cref="OutException"/> based on an exception that occured during an outgoing operation for which we do not have a valid/complete message.
        /// Example: transformation.
        /// </summary>
        /// <param name="exception">The exception which message will be inserted.</param>
        /// <param name="messageStream">The stream that represents the message which caused the exception.</param>
        public async Task InsertOutgoingExceptionAsync(Exception exception, Stream messageStream)
        {
            string location = await _bodyStore.SaveAS4MessageStreamAsync(
                _config.OutExceptionStoreLocation,
                messageStream);

            OutException entity = OutException.ForMessageBody(location, exception);
            _repository.InsertOutException(entity);
        }
    }
}
