using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Streaming;
using NLog;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using RetryReliability = Eu.EDelivery.AS4.Model.PMode.RetryReliability;

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

            StreamUtilities.MovePositionToStreamStart(context.ReceivedMessage.UnderlyingStream);

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

                    Logger.Debug(
                        $"Insert InMessage UserMessage {userMessage.MessageId} with " + 
                        $" {{Operation={inMessage.Operation}, Status={inMessage.Status}, IsTest={userMessage.IsTest}, IsDuplicate={userMessage.IsDuplicate}}}");

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

            foreach (SignalMessage signalMessage in as4Message.SignalMessages.Where(s => !(s is PullRequest)))
            {
                signalMessage.IsDuplicate = IsSignalMessageDuplicate(signalMessage, duplicateSignalMessages);

                try
                {
                    InMessage inMessage = InMessageBuilder
                        .ForSignalMessage(signalMessage, as4Message, mep)
                        .Build();

                    inMessage.MessageLocation = location;
                    inMessage.SetPModeInformation(pmode);

                    Logger.Debug(
                        $"Insert InMessage {signalMessage.GetType().Name} {signalMessage.MessageId} with " + 
                        $"{{Operation={inMessage.Operation}, Status={inMessage.Status}}}");

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
            
            if (as4Message.HasUserMessage)
            {
                IEnumerable<string> messageLocations = _repository.GetInMessagesData(
                    as4Message.UserMessages.Select(m => m.MessageId),
                    m => m.MessageLocation);

                if (!messageLocations.Any() || messageLocations.Any(m => m is null))
                {
                    throw new InvalidDataException(
                        $"Cannot update received AS4Message: Unable to find an InMessage for {as4Message.GetPrimaryMessageId()}");
                }

                Logger.Debug("Update stored message body because message contains UserMessages");
                foreach (string location in messageLocations)
                {
                    messageBodyStore.UpdateAS4Message(location, as4Message);
                }
            }

            if (messageContext.ReceivedMessageMustBeForwarded)
            {
                string pmodeString = messageContext.GetReceivingPModeString();
                string pmodeId = messageContext.ReceivingPMode?.Id;

                // Only set the Operation of the InMessage that represents the 
                // Primary Message-Unit to 'ToBeForwarded' since we want to prevent
                // that the same message is forwarded more than once (x number of messaging units 
                // present in the AS4 Message).

                _repository.UpdateInMessages(
                    m => messageContext.AS4Message.MessageIds.Contains(m.EbmsMessageId),
                    m =>
                    {
                        m.Intermediary = true;
                        m.SetPModeInformation(pmodeId, pmodeString);
                        Logger.Debug($"Update InMessage {m.EbmsMessageType} with {{Intermediary={m.Intermediary}, PMode={pmodeId}}}");
                    });

                _repository.UpdateInMessage(
                    messageContext.AS4Message.GetPrimaryMessageId(),
                    m =>
                    {
                        m.Operation = Operation.ToBeForwarded;
                        Logger.Debug($"Update InMessage {m.EbmsMessageType} with {{Operation={m.Operation}}}");
                    });
            }
            else
            {
                UpdateUserMessagesForDeliveryAndNotification(messageContext);
                UpdateSignalMessages(messageContext);
            }
        }

        private void UpdateUserMessagesForDeliveryAndNotification(MessagingContext ctx)
        {
            IEnumerable<UserMessage> userMsgs = ctx.AS4Message.UserMessages;
            if (userMsgs.Any() == false)
            {
                return;
            }

            string receivingPModeId = ctx.ReceivingPMode?.Id;
            string receivingPModeString = ctx.GetReceivingPModeString();

            var xs = _repository
                .GetInMessagesData(userMsgs.Select(um => um.MessageId), im => im.Id)
                .Zip(userMsgs, Tuple.Create);

            foreach ((long id, UserMessage userMessage) in xs)
            {
                _repository.UpdateInMessage(
                    userMessage.MessageId,
                    message =>
                    {
                        message.SetPModeInformation(receivingPModeId, receivingPModeString);

                        if (UserMessageNeedsToBeDelivered(ctx.ReceivingPMode, userMessage)
                            && message.Intermediary == false)
                        {
                            message.Operation = Operation.ToBeDelivered;

                            RetryReliability reliability =
                                ctx.ReceivingPMode.MessageHandling?.DeliverInformation?.Reliability;

                            if (reliability?.IsEnabled ?? false)
                            {
                                var r = Entities.RetryReliability.CreateForInMessage(
                                    refToInMessageId: id,
                                    maxRetryCount: reliability.RetryCount,
                                    retryInterval: reliability.RetryInterval.AsTimeSpan(),
                                    type: RetryType.Delivery);

                                _repository.InsertRetryReliability(r);
                            }

                            Logger.Debug($"Update InMessage UserMessage {userMessage.MessageId} with {{Operation={message.Operation}}}");
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
                string[] signalsToNotify = 
                    signalMessages.Where(r => r.IsDuplicate == false)
                                  .Select(s => s.MessageId)
                                  .ToArray();

                if (signalsToNotify.Any())
                {
                    _repository.UpdateInMessages(
                        m => signalsToNotify.Contains(m.EbmsMessageId) && m.Intermediary == false,
                        m =>
                        {
                            m.Operation = Operation.ToBeNotified;
                            Logger.Debug($"Update InMessage {m.EbmsMessageType} {m.EbmsMessageId} with {{Operation={m.Operation}}}");
                        });

                    bool isRetryEnabled = reliability?.IsEnabled ?? false;
                    if (isRetryEnabled)
                    {
                        IEnumerable<long> ids = _repository.GetInMessagesData(signalsToNotify, m => m.Id);
                        foreach (long id in ids)
                        {
                            var r = Entities.RetryReliability.CreateForInMessage(
                                refToInMessageId: id,
                                maxRetryCount: reliability.RetryCount,
                                retryInterval: reliability.RetryInterval.AsTimeSpan(),
                                type: RetryType.Notification);

                            _repository.InsertRetryReliability(r);
                        }
                    }
                }
            }

            string[] refToMessageIds = signalMessages.Select(r => r.RefToMessageId).Where(id => id != null).ToArray();
            if (refToMessageIds.Any())
            {
                _repository.UpdateOutMessages(
                    m => refToMessageIds.Contains(m.EbmsMessageId) && m.Intermediary == false,
                    m =>
                    {
                        m.SetStatus(outStatus);
                        Logger.Debug($"Update OutMessage UserMessage {m.EbmsMessageId} with {{Status={outStatus}}}");
                    });
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
                Logger.Debug(
                    "UserMessage will not be delivered since the " + 
                    $"ReceivingPMode {pmode.Id} has not a MessageHandling.Deliver element");

                return false;
            }

            bool needsToBeDelivered = 
                pmode.MessageHandling.DeliverInformation.IsEnabled 
                && !userMessage.IsDuplicate 
                && !userMessage.IsTest;

            Logger.Debug(
                $"UserMessage {(needsToBeDelivered ? "will" : "will not")} be delivered because the " +
                $"ReceivingPMode {pmode.Id} MessageHandling.Deliver.IsEnabled={pmode.MessageHandling.DeliverInformation.IsEnabled} and " +
                $"the UserMessage {(userMessage.IsTest ? "is" : "isn't")} a test message and {(userMessage.IsDuplicate ? "is" : "isn't")} a duplicate one");

            return needsToBeDelivered;
        }
    }
}