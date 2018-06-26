using System;
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

        /// <inheritdoc />
        public void DeadletterOutMessage(long outMessageId, string ebmsMessageId, IAS4MessageBodyStore messageBodyStore)
        {
            InMessage inMessage = CreateErrorInMessage(outMessageId, ebmsMessageId, messageBodyStore);
            _repository.InsertInMessage(inMessage);

            _repository.UpdateOutMessage(outMessageId, x => x.Operation = Operation.DeadLettered);
        }

        private InMessage CreateErrorInMessage(
            long outMessageId,
            string ebmsMessageId,
            IAS4MessageBodyStore messageBodyStore)
        {
            // TODO: should this not be an OutException instead of an InMessage ?
            //       Maybe it is an InMessage because we have more detailed information
            //       regarding the type of Error ?

            var outMessageData = _repository.GetOutMessageData(
                messageId: outMessageId,
                selection: m => new
                {
                    pmode = AS4XmlSerializer.FromString<SendingProcessingMode>(m.PMode),
                    mep = m.MEP
                });

            Error errorMessage = CreateError(ebmsMessageId);
            AS4Message as4Message = AS4Message.Create(errorMessage, outMessageData.pmode);

            // We do not use the InMessageService to persist the incoming message here, since this is not really
            // an incoming message.  We create this InMessage in order to be able to notify the Message Producer
            // if he should be notified when a message cannot be sent.
            // (Maybe we should only create the InMessage when notification is enabled ?)
            string location =
                messageBodyStore.SaveAS4Message(
                    location: _configuration.InMessageStoreLocation,
                    message: as4Message);

            InMessage inMessage =
                InMessageBuilder.ForSignalMessage(errorMessage, as4Message, outMessageData.mep)
                                .WithPMode(outMessageData.pmode)
                                .Build();

            inMessage.MessageLocation = location;

            var targetOperation = outMessageData.pmode.ErrorHandling.NotifyMessageProducer
                    ? Operation.ToBeNotified
                    : Operation.NotApplicable;

            inMessage.Operation = targetOperation;

            return inMessage;
        }

        private static Error CreateError(string messageId)
        {
            return new ErrorBuilder()
                .WithRefToEbmsMessageId(messageId)
                .WithErrorResult(new ErrorResult($"[{messageId}] Missing Receipt", ErrorAlias.MissingReceipt))
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
                messageId: awareness.RefToOutMessageId,
                selection: m => m.Status == OutStatus.Ack.ToString() || m.Status == OutStatus.Nack.ToString());
        }

        /// <summary>
        /// Messages the needs to be resend.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public bool MessageNeedsToBeResend(ReceptionAwareness awareness)
        {
            if (awareness.LastSendTime == null)
            {
                return false;
            }

            DateTimeOffset deadlineForResend = awareness.LastSendTime.Value.Add(TimeSpan.Parse(awareness.RetryInterval));

            return awareness.Status != ReceptionStatus.Completed
                   && awareness.CurrentRetryCount < awareness.TotalRetryCount
                   && DateTimeOffset.Now > deadlineForResend
                   && _repository.GetOutMessageData(messageId: awareness.RefToOutMessageId,
                                                    selection: m => m.Operation) != Operation.Sending;
        }

        /// <summary>
        /// Completes the message.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        public void MarkReferencedMessageAsComplete(ReceptionAwareness awareness)
        {
            Logger.Info($"[{awareness.RefToEbmsMessageId}] Reception Awareness completed");

            UpdateReceptionAwareness(awareness, ReceptionStatus.Completed);
        }

        /// <summary>
        /// Updates for resend.
        /// </summary>
        /// <param name="awareness">The awareness.</param>
        public void MarkReferencedMessageForResend(ReceptionAwareness awareness)
        {
            Logger.Info(
                $"[{awareness.RefToEbmsMessageId}] Update datastore so the ebMS message can be resend. (RetryCount = {awareness.CurrentRetryCount + 1})");

            _repository.UpdateOutMessage(awareness.RefToOutMessageId, m => m.Operation = Operation.ToBeSent);
            UpdateReceptionAwareness(awareness, ReceptionStatus.Pending);
        }

        /// <summary>
        /// Resets the referenced message.
        /// </summary>
        /// <param name="awarenes">The awarenes.</param>
        public void ResetReferencedMessage(ReceptionAwareness awarenes)
        {
            UpdateReceptionAwareness(awarenes, ReceptionStatus.Pending);
        }

        private void UpdateReceptionAwareness(ReceptionAwareness awarenes, ReceptionStatus receptionStatus)
        {
            _repository.UpdateReceptionAwareness(awarenes.Id, r => r.Status = receptionStatus);
        }
    }
}