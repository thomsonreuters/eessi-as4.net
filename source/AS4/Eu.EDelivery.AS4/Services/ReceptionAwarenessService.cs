using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;
using ReceptionAwareness = Eu.EDelivery.AS4.Entities.ReceptionAwareness;

namespace Eu.EDelivery.AS4.Services
{
    public class ReceptionAwarenessService : IReceptionAwarenessService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatastoreRepository _repository;
        private readonly IConfig _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public ReceptionAwarenessService(IDatastoreRepository repository) : this(Config.Instance, repository) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessService" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="repository">The repository.</param>
        public ReceptionAwarenessService(IConfig config, IDatastoreRepository repository)
        {
            _configuration = config;
            _repository = repository;
        }

        /// <summary>
        /// Deadletters the out message asynchronous.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="messageBodyStore">The message body persister.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task DeadletterOutMessageAsync(
            string messageId,
            IAS4MessageBodyStore messageBodyStore,
            CancellationToken cancellationToken)
        {
            InMessage inMessage = await CreateErrorInMessage(messageId, messageBodyStore, cancellationToken);
            _repository.InsertInMessage(inMessage);

            _repository.UpdateOutMessage(messageId, x => x.Operation = Operation.DeadLettered);
        }

        private async Task<InMessage> CreateErrorInMessage(
            string messageId,
            IAS4MessageBodyStore messageBodyStore,
            CancellationToken cancellationToken)
        {
            SendingProcessingMode pmode = _repository.GetOutMessageData(
                messageId,
                m => AS4XmlSerializer.FromString<SendingProcessingMode>(m.PMode));

            Model.Core.Error errorMessage = CreateError(messageId);
            AS4Message as4Message = AS4Message.Create(errorMessage, pmode);

            // We do not use the InMessageService to persist the incoming message here, since this is not really
            // an incoming message.  We create this InMessage in order to be able to notify the Message Producer
            // if he should be notified when a message cannot be sent.
            // (Maybe we should only create the InMessage when notification is enabled ?)
            string location = 
                await messageBodyStore.SaveAS4MessageAsync(
                    location: _configuration.InMessageStoreLocation,
                    message: as4Message,
                    cancellation: cancellationToken);

            InMessage inMessage = InMessageBuilder
                .ForSignalMessage(errorMessage, as4Message)
                .WithPModeString(await AS4XmlSerializer.ToStringAsync(pmode))
                .Build(cancellationToken);

            inMessage.MessageLocation = location;

            inMessage.Operation = pmode.ErrorHandling.NotifyMessageProducer
                    ? Operation.ToBeNotified
                    : Operation.NotApplicable;

            return inMessage;
        }

        private static Error CreateError(string messageId)
        {
            return new ErrorBuilder()
                .WithRefToEbmsMessageId(messageId)
                .WithAS4Exception(CreateAS4Exception(messageId))
                .Build();
        }

        private static AS4Exception CreateAS4Exception(string messageId)
        {
            return AS4ExceptionBuilder
                .WithDescription($"[{messageId}] Missing Receipt")
                .WithMessageIds(messageId)
                .WithErrorCode(ErrorCode.Ebms0301)
                .Build();
        }

        /// <summary>
        /// Determines whether [is message already answered] [the specified awareness].
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        /// <returns>
        ///   <c>true</c> if [is message already answered] [the specified awareness]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMessageAlreadyAnswered(ReceptionAwareness awareness)
        {
            return _repository.GetOutMessageData(
                awareness.InternalMessageId,
                m => m.Status == OutStatus.Ack || m.Status == OutStatus.Nack);
        }

        /// <summary>
        /// Messages the needs to be resend.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public bool MessageNeedsToBeResend(ReceptionAwareness awareness)
        {
            Func<DateTimeOffset> deadlineForResend =
                () => awareness.LastSendTime.Add(TimeSpan.Parse(awareness.RetryInterval));

            return awareness.Status != ReceptionStatus.Completed
                   && awareness.CurrentRetryCount < awareness.TotalRetryCount
                   && DateTimeOffset.UtcNow > deadlineForResend()
                   && _repository.GetOutMessageData(awareness.InternalMessageId, m => m.Operation) != Operation.Sending;
        }

        /// <summary>
        /// Completes the message.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        public void MarkReferencedMessageAsComplete(ReceptionAwareness awareness)
        {
            Logger.Debug("Message has been answered, marking as complete");
            Logger.Info($"[{awareness.InternalMessageId}] Reception Awareness completed");

            UpdateReceptionAwareness(awareness, ReceptionStatus.Completed);
        }

        /// <summary>
        /// Updates for resend.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        public void MarkReferencedMessageForResend(ReceptionAwareness awareness)
        {
            string messageId = awareness.InternalMessageId;
            Logger.Info(
                $"[{messageId}] Update datastore so the ebMS message can be resend. (RetryCount = {awareness.CurrentRetryCount + 1})");

            _repository.UpdateOutMessage(messageId, m => m.Operation = Operation.ToBeSent);
            UpdateReceptionAwareness(awareness, ReceptionStatus.Pending);
        }

        /// <summary>
        /// Resets the referenced message.
        /// </summary>
        /// <param name="awarenes">The awarenes.</param>
        public void ResetReferencedMessage(ReceptionAwareness awarenes)
        {
            Logger.Info($"[{awarenes.InternalMessageId}] Modify Reception Awareness Status");

            UpdateReceptionAwareness(awarenes, ReceptionStatus.Pending);
        }

        private void UpdateReceptionAwareness(ReceptionAwareness awarenes, ReceptionStatus receptionStatus)
        {
            _repository.UpdateReceptionAwareness(awarenes.InternalMessageId, r => r.Status = receptionStatus);
        }
    }
}