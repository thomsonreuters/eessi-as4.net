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
using Eu.EDelivery.AS4.Extensions;
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
        /// <param name="repository">The repository.</param>
        public InMessageService(IConfig config, IDatastoreRepository repository)
        {
            _configuration = config;
            _repository = repository;
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
        /// <param name="mep"></param>
        /// <param name="messageBodyStore"></param>
        /// <returns>A MessagingContext instance that contains the parsed AS4 Message.</returns>
        public async Task<MessagingContext> InsertAS4MessageAsync(MessagingContext context, MessageExchangePattern mep, IAS4MessageBodyStore messageBodyStore)
        {
            if (context.ReceivedMessage == null)
            {
                throw new InvalidOperationException("The MessagingContext must contain a Received Message");
            }

            // TODO: should we start the transaction here.
            string location =
                await messageBodyStore.SaveAS4MessageStreamAsync(
                    location: _configuration.InMessageStoreLocation,
                    as4MessageStream: context.ReceivedMessage.UnderlyingStream).ConfigureAwait(false);

            context.ReceivedMessage.UnderlyingStream.Position = 0;

            try
            {
                var as4Message = context.AS4Message;

                InsertUserMessages(as4Message, mep, location, context.SendingPMode);
                InsertSignalMessages(as4Message, mep, location, context.SendingPMode);

                context.ModifyContext(as4Message);
                return context;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);

                InException inException = new InException(System.Text.Encoding.UTF8.GetBytes(location), ex.Message);

                _repository.InsertInException(inException);

                return new MessagingContext(ex);
            }
        }

        private void InsertUserMessages(
            AS4Message as4Message,
            MessageExchangePattern mep,
            string location,
            SendingProcessingMode pmode)
        {
            IDictionary<string, bool> duplicateUserMessages =
                DetermineDuplicateUserMessageIds(as4Message.UserMessages.Select(m => m.MessageId));

            foreach (UserMessage userMessage in as4Message.UserMessages)
            {
                userMessage.IsTest = IsUserMessageTest(userMessage);
                userMessage.IsDuplicate = IsUserMessageDuplicate(userMessage, duplicateUserMessages);

                try
                {
                    InMessage inMessage = InMessageBuilder
                        .ForUserMessage(userMessage, as4Message, mep)
                        .Build();

                    inMessage.SetPModeInformation(pmode);
                    inMessage.MessageLocation = location;

                    _repository.InsertInMessage(inMessage);
                }
                catch (Exception ex)
                {
                    string description = $"Unable to update UserMessage {userMessage.MessageId}";
                    Logger.Error(description);

                    throw new DataException(description, ex);
                }
            }
        }

        private void InsertSignalMessages(
            AS4Message as4Message,
            MessageExchangePattern mep,
            string location,
            SendingProcessingMode pmode)
        {
            if (!as4Message.SignalMessages.Any())
            {
                return;
            }

            IEnumerable<string> relatedUserMessageIds = as4Message.SignalMessages
                .Select(m => m.RefToMessageId)
                .Where(refToMessageId => !String.IsNullOrWhiteSpace(refToMessageId));

            IDictionary<string, bool> duplicateSignalMessages =
                DetermineDuplicateSignalMessageIds(relatedUserMessageIds);

            foreach (SignalMessage signalMessage in as4Message.SignalMessages)
            {
                signalMessage.IsDuplicate = IsSignalMessageDuplicate(signalMessage, duplicateSignalMessages);

                try
                {
                    InMessage inMessage = InMessageBuilder
                        .ForSignalMessage(signalMessage, as4Message, mep)
                        .Build();

                    inMessage.MessageLocation = location;
                    inMessage.SetPModeInformation(pmode);

                    _repository.InsertInMessage(inMessage);
                }
                catch (Exception exception)
                {
                    string description = $"Unable to update SignalMessage {signalMessage.MessageId}";
                    Logger.Error(description);

                    throw new DataException(description, exception);
                }
            }
        }

        /// <summary>Updates an <see cref="AS4Message"/> for delivery and notification.</summary>
        /// <param name="messageContext">The message Context.</param>
        /// <param name="messageBodyStore">The as4 message body persister.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public void UpdateAS4MessageForMessageHandling(MessagingContext messageContext, IAS4MessageBodyStore messageBodyStore)
        {
            AS4Message as4Message = messageContext.AS4Message;

            string messageLocation = _repository.GetInMessageData(as4Message.GetPrimaryMessageId(),
                                                                  m => m.MessageLocation);

            if (messageLocation == null)
            {
                throw new InvalidDataException($"Cannot update received AS4Message: Unable to find an InMessage for {as4Message.GetPrimaryMessageId()}");
            }

            if (as4Message.IsUserMessage)
            {
                messageBodyStore.UpdateAS4Message(messageLocation, as4Message);
            }

            if (messageContext.ReceivedMessageMustBeForwarded)
            {
                var pmodeString = messageContext.GetReceivingPModeString();
                var pmodeId = messageContext.ReceivingPMode?.Id;

                // Only set the Operation of the InMessage that represents the 
                // Primary Message-Unit to 'ToBeForwarded' since we want to prevent
                // that the same message is forwarded more than once (x number of messaging units 
                // present in the AS4 Message).

                _repository.UpdateInMessages(m => messageContext.AS4Message.MessageIds.Contains(m.EbmsMessageId),
                                             m =>
                                             {
                                                 m.Intermediary = true;
                                                 m.SetPModeInformation(pmodeId, pmodeString);
                                             });
                _repository.UpdateInMessage(messageContext.AS4Message.GetPrimaryMessageId(),
                                            m =>
                                            {
                                                m.SetOperation(Operation.ToBeForwarded);
                                            });
            }
            else
            {
                UpdateUserMessagesForDeliveryAndNotification(messageContext);
                UpdateSignalMessages(messageContext);
            }
        }

        private void UpdateUserMessagesForDeliveryAndNotification(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message.UserMessages.Any() == false)
            {
                return;
            }

            string receivingPModeId = messagingContext.ReceivingPMode?.Id;
            string receivingPModeString = messagingContext.GetReceivingPModeString();

            foreach (UserMessage userMessage in messagingContext.AS4Message.UserMessages)
            {
                _repository.UpdateInMessage(
                    userMessage.MessageId,
                    message =>
                    {
                        message.SetPModeInformation(receivingPModeId, receivingPModeString);

                        if (UserMessageNeedsToBeDelivered(messagingContext.ReceivingPMode, userMessage) 
                            && message.Intermediary == false)
                        {
                            message.SetOperation(Operation.ToBeDelivered);

                            RetryReliability reliability = 
                                messagingContext.ReceivingPMode.MessageHandling?.DeliverInformation?.Reliability;

                            if (reliability?.IsEnabled ?? false)
                            {
                                message.CurrentRetryCount = 0;
                                message.MaxRetryCount = reliability.RetryCount;
                                message.SetRetryInterval(reliability.RetryInterval.AsTimeSpan());
                            }

                        }
                    });
            }
        }

        private void UpdateSignalMessages(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;

            // Improvement: I think it will be safer if we retrieve the sending-pmodes of the related usermessages ourselves here
            // instead of relying on the SendingPMode that is available in the AS4Message object (which is set by another Step in the queue).
            IEnumerable<Receipt> receipts = as4Message.SignalMessages.OfType<Receipt>();
            bool notifyReceipts = messagingContext.SendingPMode?.ReceiptHandling?.NotifyMessageProducer ?? false;
            RetryReliability retryReceipts = messagingContext.SendingPMode?.ReceiptHandling?.Reliability;
            UpdateSignalMessages(receipts, () => notifyReceipts, OutStatus.Ack, retryReceipts);

            IEnumerable<Error> errors = as4Message.SignalMessages.OfType<Error>();
            bool notifyErrors = messagingContext.SendingPMode?.ErrorHandling?.NotifyMessageProducer ?? false;
            RetryReliability retryErrors = messagingContext.SendingPMode?.ErrorHandling?.Reliability;
            UpdateSignalMessages(errors, () => notifyErrors, OutStatus.Nack, retryErrors);
        }

        private void UpdateSignalMessages(
            IEnumerable<SignalMessage> signalMessages, 
            Func<bool> signalsMustBeNotified, 
            OutStatus outStatus,
            RetryReliability reliability)
        {
            if (signalsMustBeNotified())
            {
                var signalsToNotify = signalMessages.Where(r => r.IsDuplicate == false).Select(s => s.MessageId).ToArray();

                if (signalsToNotify.Any())
                {
                    _repository.UpdateInMessages(
                        m => signalsToNotify.Contains(m.EbmsMessageId) && m.Intermediary == false,
                        m =>
                        {
                            m.SetOperation(Operation.ToBeNotified);

                            bool isRetryEnabled = reliability?.IsEnabled ?? false;
                            if (isRetryEnabled)
                            {
                                m.CurrentRetryCount = 0;
                                m.MaxRetryCount = reliability.RetryCount;
                                m.SetRetryInterval(reliability.RetryInterval.AsTimeSpan());
                            }

                        });
                }
            }

            var refToMessageIds = signalMessages.Select(r => r.RefToMessageId).ToArray();

            if (refToMessageIds.Any())
            {
                _repository.UpdateOutMessages(
                    m => refToMessageIds.Contains(m.EbmsMessageId) && m.Intermediary == false,
                    m => m.SetStatus(outStatus));
            }
        }

        #region UserMessage related

        private static bool IsUserMessageTest(UserMessage userMessage)
        {
            CollaborationInfo collaborationInfo = userMessage.CollaborationInfo;

            bool isTestMessage = (collaborationInfo.Service.Value?.Equals(Constants.Namespaces.TestService) ?? false) &&
                                 (collaborationInfo.Action?.Equals(Constants.Namespaces.TestAction) ?? false);

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

        #endregion

        #region SignalMessage related

        private static bool IsSignalMessageDuplicate(
            MessageUnit signalMessage,
            IDictionary<string, bool> duplicateSignalMessages)
        {
            if (string.IsNullOrWhiteSpace(signalMessage.RefToMessageId))
            {
                return false;
            }

            duplicateSignalMessages.TryGetValue(signalMessage.RefToMessageId, out bool isDuplicate);

            if (isDuplicate)
            {
                Logger.Info($"[{signalMessage.RefToMessageId}] Incoming Signal Message is a duplicated one");
            }

            return isDuplicate;
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