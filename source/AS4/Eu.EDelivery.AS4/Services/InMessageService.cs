using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Repository to expose Data store related operations
    /// for the Update Data store Steps
    /// </summary>
    public class InMessageService : IInMessageService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatastoreRepository _repository;
        private readonly IConfig _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageService"/> class. 
        /// Create a new Data store Repository
        /// </summary>
        /// <param name="repository"></param>
        public InMessageService(IDatastoreRepository repository) : this(Config.Instance, repository) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageService"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="respository">The respository.</param>
        public InMessageService(IConfig config, IDatastoreRepository respository)
        {
            _configuration = config;
            _repository = respository;
        }

        /// <summary>
        /// Search for duplicate <see cref="UserMessage"/> instances in the configured datastore for the given <paramref name="searchedMessageIds"/>.
        /// </summary>
        /// <param name="searchedMessageIds">'EbmsMessageIds' to search for duplicates.</param>
        /// <returns></returns>
        public IDictionary<string, bool> DetermineDuplicateUserMessageIds(IEnumerable<string> searchedMessageIds)
        {
            IEnumerable<string> duplicateMessageIds = _repository.SelectExistingInMessageIds(searchedMessageIds);

            return MergeTwoListsIntoADuplicateMessageMapping(searchedMessageIds, duplicateMessageIds);
        }

        /// <summary>
        /// Search for duplicate <see cref="SignalMessage"/> instances in the configured datastore for the given <paramref name="searchedMessageIds"/>.
        /// </summary>
        /// <param name="searchedMessageIds">'RefToEbmsMessageIds' to search for duplicates.</param>
        /// <returns></returns>
        public IDictionary<string, bool> DetermineDuplicateSignalMessageIds(IEnumerable<string> searchedMessageIds)
        {
            IEnumerable<string> duplicateMessageIds = _repository.SelectExistingRefInMessageIds(searchedMessageIds);

            return MergeTwoListsIntoADuplicateMessageMapping(searchedMessageIds, duplicateMessageIds);
        }

        private static IDictionary<string, bool> MergeTwoListsIntoADuplicateMessageMapping(
            IEnumerable<string> searchedMessageIds,
            IEnumerable<string> duplicateMessageIds)
        {
            return
                searchedMessageIds.Select(i => new KeyValuePair<string, bool>(i, duplicateMessageIds.Contains(i)))
                                  .ToDictionary(k => k.Key, v => v.Value);
        }


        /// <summary>
        /// Inserts a received Message in the DataStore.
        /// For each message-unit that exists in the AS4Message,an InMessage record is created.
        /// The AS4 Message Body is persisted as it has been received.
        /// </summary>
        /// <remarks>The received Message is parsed to an AS4 Message instance.</remarks>
        /// <param name="context"></param>
        /// <param name="messageBodyStore"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A MessagingContext instance that contains the parsed AS4 Message.</returns>
        public async Task<MessagingContext> InsertAS4Message(
            MessagingContext context,
            MessageExchangePattern mep,
            IAS4MessageBodyStore messageBodyStore,
            CancellationToken cancellationToken)
        {
            if (context.ReceivedMessage == null)
            {
                throw new InvalidOperationException("The MessagingContext must contain a Received Message");
            }

            // TODO: should we start the transaction here.
            string location =
                await messageBodyStore.SaveAS4MessageStreamAsync(
                    location: _configuration.InMessageStoreLocation,
                    as4MessageStream: context.ReceivedMessage.UnderlyingStream,
                    cancellation: cancellationToken);

            try
            {
                context.ReceivedMessage.UnderlyingStream.Position = 0;

                var deserializer = SerializerProvider.Default.Get(context.ReceivedMessage.ContentType);

                var as4Message = await deserializer.DeserializeAsync(context.ReceivedMessage.UnderlyingStream, context.ReceivedMessage.ContentType,
                                                                     cancellationToken);


                InsertUserMessages(as4Message, mep, location, cancellationToken);
                InsertSignalMessages(as4Message, mep, location, cancellationToken);

                context.ModifyContext(as4Message);

                return context;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                InException inException = new InException();
                inException.Exception = ex.Message;
                inException.MessageBody = System.Text.Encoding.UTF8.GetBytes(location);
                _repository.InsertInException(inException);

                return new MessagingContext(ex);
            }
        }

        private void InsertUserMessages(AS4Message as4Message, MessageExchangePattern mep, string location, CancellationToken cancellationToken)
        {
            IDictionary<string, bool> duplicateUserMessages =
                DetermineDuplicateUserMessageIds(as4Message.UserMessages.Select(m => m.MessageId));

            foreach (UserMessage userMessage in as4Message.UserMessages)
            {
                userMessage.IsTest = IsUserMessageTest(userMessage);
                userMessage.IsDuplicate = IsUserMessageDuplicate(userMessage, duplicateUserMessages);

                AttemptToInsertUserMessage(userMessage, as4Message, mep, location, cancellationToken);
            }
        }

        private void InsertSignalMessages(
            AS4Message as4Message,
            MessageExchangePattern mep,
            string location,
            CancellationToken cancellationToken)
        {
            if (as4Message.SignalMessages.Any())
            {
                IEnumerable<string> relatedUserMessageIds = as4Message.SignalMessages.Select(m => m.RefToMessageId);

                IDictionary<string, bool> duplicateSignalMessages =
                    DetermineDuplicateSignalMessageIds(relatedUserMessageIds);

                foreach (SignalMessage signalMessage in as4Message.SignalMessages)
                {
                    signalMessage.IsDuplicate = IsSignalMessageDuplicate(signalMessage, duplicateSignalMessages);

                    AttemptToInsertSignalMessage(signalMessage, as4Message, mep, location, cancellationToken);
                }
            }
        }

        /// <summary>Updates an <see cref="AS4Message"/> for delivery and notification.</summary>
        /// <param name="messageContext">The message Context.</param>
        /// <param name="messageBodyStore">The as4 message body persister.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task UpdateAS4MessageForDeliveryAndNotification(
            MessagingContext messageContext,
            IAS4MessageBodyStore messageBodyStore,
            CancellationToken cancellationToken)
        {
            AS4Message as4Message = messageContext.AS4Message;
            string messageLocation = _repository.GetInMessageData(
                as4Message.GetPrimaryMessageId(),
                m => m.MessageLocation);

            if (messageLocation == null)
            {
                throw new InvalidDataException($"Unable to find an InMessage for {as4Message.GetPrimaryMessageId()}");
            }

            if (as4Message.IsUserMessage)
            {
                await messageBodyStore.UpdateAS4MessageAsync(messageLocation, as4Message, cancellationToken);
                UpdateUserMessagesForDeliveryAndNotification(messageContext);
            }

            UpdateSignalMessages(messageContext);
        }

        private void UpdateUserMessagesForDeliveryAndNotification(MessagingContext messagingContext)
        {
            string receivingPModeString = messagingContext.GetReceivingPModeString();

            bool userMessageNeedsToBeDelivered = UserMessageNeedsToBeDelivered(
                messagingContext.ReceivingPMode,
                messagingContext.AS4Message.PrimaryUserMessage);

            Action<InMessage> updateOperation = NeedsToBeDeliveredIf()(userMessageNeedsToBeDelivered);

            foreach (UserMessage userMessage in messagingContext.AS4Message.UserMessages)
            {
                _repository.UpdateInMessage(
                    userMessage.MessageId,
                    message =>
                    {
                        message.PMode = receivingPModeString;
                        updateOperation(message);
                    });
            }
        }

        private static Func<bool, Action<InMessage>> NeedsToBeDeliveredIf()
        {
            return needsToBeNotified =>
            {
                return m =>
                {
                    if (needsToBeNotified)
                    {
                        m.Operation = Operation.ToBeDelivered;
                    }
                };
            };
        }

        private void UpdateSignalMessages(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;

            // Improvement: I think it will be safer if we retrieve the sending-pmodes of the related usermessages ourselves here
            // instead of relying on the SendingPMode that is available in the AS4Message object (which is set by another Step in the queueu).
            foreach (SignalMessage signalMessage in as4Message.SignalMessages)
            {
                if (signalMessage is Receipt)
                {
                    if (ReceiptMustBeNotified(messagingContext.SendingPMode) && signalMessage.IsDuplicate == false)
                    {
                        _repository.UpdateInMessage(signalMessage.MessageId, r => r.Operation = Operation.ToBeNotified);
                    }

                    UpdateRefUserMessageStatus(signalMessage, OutStatus.Ack);
                }
                else if (signalMessage is Error)
                {
                    if (ErrorMustBeNotified(messagingContext.SendingPMode) && signalMessage.IsDuplicate == false)
                    {
                        _repository.UpdateInMessage(signalMessage.MessageId, r => r.Operation = Operation.ToBeNotified);
                    }

                    UpdateRefUserMessageStatus(signalMessage, OutStatus.Nack);
                }
            }
        }

        #region UserMessage related

        private static bool IsUserMessageTest(UserMessage userMessage)
        {
            CollaborationInfo collaborationInfo = userMessage.CollaborationInfo;

            bool isTestMessage = collaborationInfo.Service.Value.Equals(Constants.Namespaces.TestService)
                                 && collaborationInfo.Action.Equals(Constants.Namespaces.TestAction);

            if (isTestMessage)
            {
                Logger.Info($"[{userMessage.MessageId}] Incoming User Message is 'Test Message'");
            }

            return isTestMessage;
        }

        private static bool IsUserMessageDuplicate(
            MessageUnit userMessage,
            IDictionary<string, bool> duplicateUserMessages)
        {
            duplicateUserMessages.TryGetValue(userMessage.MessageId, out bool isDuplicate);

            if (isDuplicate)
            {
                Logger.Info($"[{userMessage.MessageId}] Incoming User Message is a duplicated one");
            }

            return isDuplicate;
        }

        private void AttemptToInsertUserMessage(
            UserMessage userMessage,
            AS4Message belongsToAS4Message,
            MessageExchangePattern mep,
            string location,
            CancellationToken cancellationToken)
        {
            try
            {
                InMessage inMessage = CreateUserInMessage(userMessage, belongsToAS4Message, mep, location, cancellationToken);
                _repository.InsertInMessage(inMessage);
            }
            catch (Exception ex)
            {
                string description = $"Unable to update UserMessage {userMessage.MessageId}";
                Logger.Error(description);

                throw new DataException(description, ex);
            }
        }

        private static InMessage CreateUserInMessage(
            UserMessage userMessage,
            AS4Message belongsToAS4Message,
            MessageExchangePattern mep,
            string messageLocation,
            CancellationToken cancellationToken)
        {
            InMessage inMessage =
                InMessageBuilder.ForUserMessage(userMessage, belongsToAS4Message, mep)
                                .Build(cancellationToken);

            inMessage.MessageLocation = messageLocation;

            return inMessage;
        }

        #endregion

        #region SignalMessage related

        private static bool IsSignalMessageDuplicate(
            MessageUnit signalMessage,
            IDictionary<string, bool> duplicateSignalMessages)
        {
            duplicateSignalMessages.TryGetValue(signalMessage.RefToMessageId, out bool isDuplicate);

            if (isDuplicate)
            {
                Logger.Info($"[{signalMessage.RefToMessageId}] Incoming Signal Message is a duplicated one");
            }

            return isDuplicate;
        }

        private void AttemptToInsertSignalMessage(
            SignalMessage signalMessage,
            AS4Message as4Message,
            MessageExchangePattern mep,
            string location,
            CancellationToken cancellationToken)
        {
            try
            {
                InMessage inMessage =
                InMessageBuilder.ForSignalMessage(signalMessage, as4Message, mep)
                                .Build(cancellationToken);
                inMessage.MessageLocation = location;

                _repository.InsertInMessage(inMessage);
            }
            catch (Exception exception)
            {
                string description = $"Unable to update SignalMessage {signalMessage.MessageId}";
                Logger.Error(description);

                throw new DataException(description, exception);
            }
        }

        private static bool ReceiptMustBeNotified(SendingProcessingMode sendingPMode)
        {
            return sendingPMode?.ReceiptHandling?.NotifyMessageProducer ?? false;
        }

        private static bool ErrorMustBeNotified(SendingProcessingMode sendingPMode)
        {
            return sendingPMode?.ErrorHandling?.NotifyMessageProducer ?? true;
        }

        private void UpdateRefUserMessageStatus(MessageUnit signalMessage, OutStatus status)
        {
            if (status == OutStatus.NotApplicable)
            {
                return;
            }

            _repository.UpdateOutMessage(signalMessage.RefToMessageId, outMessage => outMessage.Status = status);
        }

        #endregion SignalMessage related

        private static bool UserMessageNeedsToBeDelivered(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            if (pmode.MessageHandling?.DeliverInformation == null)
            {
                return false;
            }

            return pmode.MessageHandling.DeliverInformation.IsEnabled && !userMessage.IsDuplicate && !userMessage.IsTest;
        }
    }
}