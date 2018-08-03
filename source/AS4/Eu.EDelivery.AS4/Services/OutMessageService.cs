using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using ReceptionAwareness = Eu.EDelivery.AS4.Model.PMode.ReceptionAwareness;

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

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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
        /// Gets AS4 UserMessages for identifiers.
        /// </summary>
        /// <param name="messageIds">The message identifiers.</param>
        /// <param name="store">The provider.</param>
        /// <returns></returns>
        public async Task<IEnumerable<AS4Message>> GetNonIntermediaryAS4UserMessagesForIds(
            IEnumerable<string> messageIds,
            IAS4MessageBodyStore store)
        {
            IEnumerable<OutMessage> messages = _repository
                .GetOutMessageData(m =>
                    messageIds.Contains(m.EbmsMessageId) && m.Intermediary == false,
                    m => m)
                .Where(m => m != null);

            if (!messages.Any())
            {
                return Enumerable.Empty<AS4Message>();
            }

            var foundMessages = new List<AS4Message>();

            foreach (OutMessage m in messages)
            {
                Stream body = await store.LoadMessageBodyAsync(m.MessageLocation);

                ISerializer serializer = Registry.Instance.SerializerProvider.Get(m.ContentType);
                AS4Message foundMessage = await serializer.DeserializeAsync(body, m.ContentType, CancellationToken.None);

                foundMessages.Add(foundMessage);
            }

            return foundMessages;
        }

        /// <summary>
        /// Inserts a s4 message.
        /// </summary>
        /// <param name="messagingContext">The messaging context.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        public void InsertAS4Message(MessagingContext messagingContext, Operation operation)
        {
            AS4Message message = messagingContext.AS4Message;
            string messageBodyLocation =
                _messageBodyStore.SaveAS4Message(
                    location: _configuration.OutMessageStoreLocation,
                    message: message);

            Dictionary<string, MessageExchangePattern> relatedInMessageMeps =
                _repository.GetInMessagesData(
                               message.SignalMessages
                                      .Select(s => s.RefToMessageId)
                                      .Distinct(), 
                               inMsg => new { inMsg.EbmsMessageId, inMsg.MEP })
                           .Distinct()
                           .ToDictionary(r => r.EbmsMessageId, r => r.MEP);

            foreach (var messageUnit in message.MessageUnits)
            {
                var sendingPMode = GetSendingPMode(messageUnit is SignalMessage, messagingContext);

                OutMessage outMessage =
                    CreateOutMessageForMessageUnit(
                        messageUnit: messageUnit,
                        messageContext: messagingContext,
                        sendingPMode: sendingPMode,
                        relatedInMessageMeps: relatedInMessageMeps,
                        location: messageBodyLocation,
                        operation: operation);

                Logger.Debug($"Insert OutMessage {outMessage.EbmsMessageType} with {{Operation={outMessage.Operation}, Status={outMessage.Status}}}");
                _repository.InsertOutMessage(outMessage);
            }
        }

        private static OutMessage CreateOutMessageForMessageUnit(
            MessageUnit messageUnit,
            MessagingContext messageContext,
            SendingProcessingMode sendingPMode,
            Dictionary<string, MessageExchangePattern> relatedInMessageMeps,
            string location,
            Operation operation)
        {
            OutMessage outMessage =
                OutMessageBuilder.ForMessageUnit(messageUnit, messageContext.AS4Message.ContentType, sendingPMode)
                                 .Build();

            outMessage.MessageLocation = location;

            if (outMessage.EbmsMessageType == MessageType.UserMessage)
            {
                outMessage.Operation = operation;
            }
            else
            {
                MessageExchangePattern? inMessageMep = null;

                string refToMessageId = messageUnit.RefToMessageId ?? string.Empty;

                if (relatedInMessageMeps.ContainsKey(refToMessageId))
                {
                    inMessageMep = relatedInMessageMeps[refToMessageId];
                }

                (OutStatus status, Operation operation) replyPattern =
                    DetermineCorrectReplyPattern(messageContext.ReceivingPMode, inMessageMep);

                outMessage.SetStatus(replyPattern.status);
                outMessage.Operation = replyPattern.operation;
            }

            return outMessage;
        }

        private SendingProcessingMode GetSendingPMode(bool isSignalMessage, MessagingContext context)
        {
            if (context.SendingPMode?.Id != null)
            {
                return context.SendingPMode;
            }

            ReceivingProcessingMode receivePMode = context.ReceivingPMode;

            if (isSignalMessage && receivePMode != null && receivePMode.ReplyHandling.ReplyPattern == ReplyPattern.Callback)
            {
                return _configuration.GetSendingPMode(receivePMode.ReplyHandling.SendingPMode);
            }

            return null;
        }

        private static (OutStatus, Operation) DetermineCorrectReplyPattern(ReceivingProcessingMode receivingPMode, MessageExchangePattern? inMessageMep)
        {
            if (inMessageMep == null)
            {
                return (OutStatus.Created, Operation.NotApplicable);
            }

            bool isCallback = receivingPMode?.ReplyHandling?.ReplyPattern == ReplyPattern.Callback;
            bool userMessageReceivedViaPulling = inMessageMep == MessageExchangePattern.Pull;

            Operation operation = isCallback || userMessageReceivedViaPulling ? Operation.ToBeSent : Operation.NotApplicable;
            OutStatus status = isCallback || userMessageReceivedViaPulling ? OutStatus.Created : OutStatus.Sent;

            return (status, operation);
        }

        /// <summary>
        /// Updates a <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="outMessageId">The Id that uniquely identifies the OutMessage record in the database.</param>
        /// <param name="message">The message to be sent.</param>
        /// <param name="awareness">The reliability reception awareness used during the sending of the message</param>
        /// <returns></returns>
        public void UpdateAS4MessageToBeSent(
            long outMessageId,
            AS4Message message,
            ReceptionAwareness awareness)
        {
            string messageBodyLocation =
                _repository.GetOutMessageData(outMessageId, m => m.MessageLocation);

            _messageBodyStore.UpdateAS4Message(messageBodyLocation, message);

            _repository.UpdateOutMessage(
                outMessageId,
                m =>
                {
                    m.Operation = Operation.ToBeSent;
                    m.MessageLocation = messageBodyLocation;

                    if (awareness?.IsEnabled ?? false)
                    {
                        var r = Entities.RetryReliability.CreateForOutMessage(
                            outMessageId,
                            awareness.RetryCount,
                            awareness.RetryInterval.AsTimeSpan(),
                            RetryType.Send);

                        _repository.InsertRetryReliability(r);
                    }
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
        /// <returns></returns>
        void InsertAS4Message(MessagingContext message, Operation operation);
    }
}