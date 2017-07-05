using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Repository to expose Data store related operations
    /// for the Exception Handling Decorator Steps
    /// </summary>
    public class OutMessageService : IOutMessageService
    {
        private readonly IDatastoreRepository _repository;
        private readonly IAS4MessageBodyStore _messageBodyStore;
        private readonly IConfig _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService"/> class. 
        /// Create a new Insert Data store Repository
        /// with a given Data store
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore"/> that must be used to persist the AS4 Message Body.</param>
        public OutMessageService(IDatastoreRepository repository, IAS4MessageBodyStore messageBodyStore)
            : this(Config.Instance, repository, messageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="respository">The respository.</param>
        /// <param name="messageBodyStore">The as4 message body persister.</param>
        public OutMessageService(IConfig config, IDatastoreRepository respository, IAS4MessageBodyStore messageBodyStore)
        {
            _configuration = config;
            _repository = respository;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Inserts a s4 message.
        /// </summary>
        /// <param name="messagingContext">The messaging context.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task InsertAS4Message(
            MessagingContext messagingContext,
            Operation operation,
            CancellationToken cancellationToken)
        {
            AS4Message message = messagingContext.AS4Message;
            string messageBodyLocation =
                await _messageBodyStore.SaveAS4MessageAsync(
                    location: _configuration.OutMessageStoreLocation,
                    message: message,
                    cancellation: cancellationToken);

            var messageUnits = new List<MessageUnit>();
            messageUnits.AddRange(message.UserMessages);
            messageUnits.AddRange(message.SignalMessages);

            foreach (var messageUnit in messageUnits)
            {
                var sendingPMode = GetSendingPMode(messagingContext, DetermineSignalMessageType(messageUnit));

                OutMessage outMessage =
                    CreateOutMessageForMessageUnit(
                        messageUnit: messageUnit,
                        messageContext: messagingContext,
                        sendingPMode: sendingPMode,
                        location: messageBodyLocation,
                        operation: operation);

                _repository.InsertOutMessage(outMessage);
            }
        }

        private static OutMessage CreateOutMessageForMessageUnit(
            MessageUnit messageUnit,
           MessagingContext messageContext,
           SendingProcessingMode sendingPMode,
            string location,
            Operation operation)
        {
            OutMessage outMessage = OutMessageBuilder.ForMessageUnit(messageUnit, messageContext.AS4Message)
                                                     .WithSendingPMode(sendingPMode)
                                                     .Build(CancellationToken.None);

            outMessage.MessageLocation = location;

            if (outMessage.EbmsMessageType == MessageType.UserMessage)
            {
                outMessage.Operation = operation;
            }
            else
            {
                (OutStatus status, Operation operation) replyPattern =
                    DetermineCorrectReplyPattern(outMessage.EbmsMessageType, messageContext);

                outMessage.Status = replyPattern.status;
                outMessage.Operation = replyPattern.operation;
            }

            return outMessage;
        }
        private static MessageType DetermineSignalMessageType(MessageUnit messageUnit)
        {
            if (messageUnit is UserMessage)
            {
                return MessageType.UserMessage;
            }

            if (messageUnit is Receipt)
            {
                return MessageType.Receipt;
            }

            if (messageUnit is Error)
            {
                return MessageType.Error;
            }

            throw new NotSupportedException($"There exists no MessageType mapping for the specified MessageUnit type {typeof(MessageUnit)}");
        }

        private static SendingProcessingMode GetSendingPMode(MessagingContext context, MessageType messageType)
        {
            if (context.SendingPMode?.Id != null)
            {
                return context.SendingPMode;
            }

            ReceivingProcessingMode receivePMode = context.ReceivingPMode;
            // TODO: review this, needs to be modified: ReceiptHandling & ErrorHandling will not have seperate replypatterns.
            // we can check on 'is SignalMessage' instead; and than the messageType argument is no longer needed here.
            if (messageType == MessageType.Receipt && receivePMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ReceiptHandling.SendingPMode);
            }

            if (messageType == MessageType.Error && receivePMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback)
            {
                return Config.Instance.GetSendingPMode(receivePMode.ErrorHandling.SendingPMode);
            }

            return null;
        }

        private static (OutStatus, Operation) DetermineCorrectReplyPattern(
            MessageType outMessageType,
            MessagingContext message)
        {
            bool isCallback = outMessageType == MessageType.Error
                                  ? IsErrorReplyPatternCallback(message)
                                  : IsReceiptReplyPatternCallback(message);

            Operation operation = isCallback ? Operation.ToBeSent : Operation.NotApplicable;
            OutStatus status = isCallback ? OutStatus.Created : OutStatus.Sent;

            return (status, operation);
        }

        private static bool IsErrorReplyPatternCallback(MessagingContext message)
        {
            return message.ReceivingPMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private static bool IsReceiptReplyPatternCallback(MessagingContext message)
        {
            return message.ReceivingPMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;
        }

        /// <summary>
        /// Updates a <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public async Task UpdateAS4MessageToBeSent(AS4Message message, CancellationToken cancellation)
        {
            string messageBodyLocation = _repository.GetOutMessageData(message.GetPrimaryMessageId(), m => m.MessageLocation);
            await _messageBodyStore.UpdateAS4MessageAsync(messageBodyLocation, message, cancellation);

            _repository.UpdateOutMessage(
                message.GetPrimaryMessageId(),
                m =>
                {
                    m.Operation = Operation.ToBeSent;
                    m.MessageLocation = messageBodyLocation;
                });
        }
    }

    public interface IOutMessageService
    {
        /// <summary>
        /// Inserts a s4 message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        Task InsertAS4Message(MessagingContext message, Operation operation, CancellationToken cancellation);

        /// <summary>
        /// Updates a <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        Task UpdateAS4MessageToBeSent(AS4Message message, CancellationToken cancellation);
    }
}