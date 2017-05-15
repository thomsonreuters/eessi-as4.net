using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Repository to expose Data store related operations
    /// for the Update Data store Steps
    /// </summary>
    public class InMessageService : IInMessageService
    {
        private readonly IDatastoreRepository _repository;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageService"/> class. 
        /// Create a new Data store Repository
        /// </summary>
        /// <param name="repository"></param>
        public InMessageService(IDatastoreRepository repository)
        {
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
            IEnumerable<string> searchedMessageIds, IEnumerable<string> duplicateMessageIds)
        {
            return searchedMessageIds
                .Select(i => new KeyValuePair<string, bool>(i, duplicateMessageIds.Contains(i)))
                .ToDictionary(k => k.Key, v => v.Value);
        }

        public async Task InsertAS4Message(AS4Message as4Message, IAS4MessageBodyPersister as4MessageBodyPersister, CancellationToken cancellationToken)
        {
            // TODO: should we start the transaction here.
            string location = await as4MessageBodyPersister.SaveAS4MessageAsync(as4Message, cancellationToken);

            IDictionary<string, bool> duplicateUserMessages =
                this.DetermineDuplicateUserMessageIds(as4Message.UserMessages.Select(m => m.MessageId));


            foreach (UserMessage userMessage in as4Message.UserMessages)
            {
                userMessage.IsTest = IsUserMessageTest(userMessage);
                userMessage.IsDuplicate = IsUserMessageDuplicate(userMessage, duplicateUserMessages);

                AttemptToInsertUserMessage(userMessage, as4Message, location, cancellationToken);
            }

            if (as4Message.SignalMessages.Any())
            {
                var relatedUserMessageIds = as4Message.SignalMessages.Select(m => m.RefToMessageId);

                IDictionary<string, bool> duplicateSignalMessages = this.DetermineDuplicateSignalMessageIds(relatedUserMessageIds);

                // We need to retrieve the (Sending) PModes that have been used to send the user-messages for which we've 
                // received those signal-messages.

                // Without EF, this could be performed much faster using a direct SQL Update statement which links the 
                // out- & inmessage table.            
                var sendingPModeLookup = RetrieveSendingPModeInformation(relatedUserMessageIds);

                foreach (SignalMessage signalMessage in as4Message.SignalMessages)
                {
                    signalMessage.IsDuplicated = IsSignalMessageDuplicate(signalMessage, duplicateSignalMessages);

                    SendingPModeInformation sendingPModeInfo;
                    sendingPModeLookup.TryGetValue(signalMessage.RefToMessageId, out sendingPModeInfo);

                    AttemptToInsertSignalMessage(signalMessage, as4Message, sendingPModeInfo, location, cancellationToken);
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

        private static bool IsUserMessageDuplicate(MessageUnit userMessage, IDictionary<string, bool> duplicateUserMessages)
        {
            duplicateUserMessages.TryGetValue(userMessage.MessageId, out var isDuplicate);

            if (isDuplicate)
            {
                Logger.Info($"[{userMessage.MessageId}] Incoming User Message is a duplicated one");
            }

            return isDuplicate;
        }

        private void AttemptToInsertUserMessage(UserMessage userMessage, AS4Message as4Message, string location, CancellationToken cancellationToken)
        {
            try
            {
                InMessage inMessage = CreateUserInMessage(userMessage, as4Message, location, cancellationToken);
                _repository.InsertInMessage(inMessage);
            }
            catch (Exception ex)
            {
                ThrowAS4Exception($"Unable to update UserMessage {userMessage.MessageId}", as4Message, ex);
            }
        }

        private static InMessage CreateUserInMessage(
             UserMessage userMessage, AS4Message as4Message, string messageLocation, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForUserMessage(userMessage, as4Message)
                                                  .WithPModeString(as4Message.GetReceivingPModeString())
                                                  .Build(cancellationToken);

            inMessage.MessageLocation = messageLocation;


            return inMessage;
        }

        #endregion

        #region SignalMessage related

        private sealed class SendingPModeInformation
        {

            public SendingPModeInformation(string sendingPModeString)
            {
                this.SendingPModeString = sendingPModeString;
            }

            public string SendingPModeString { get; }

            private SendingProcessingMode _sendingPMode;

            public SendingProcessingMode SendingPMode
            {
                get
                {
                    if (_sendingPMode == null)
                    {
                        _sendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(SendingPModeString);
                    }
                    return _sendingPMode;
                }
            }
        }

        private Dictionary<string, SendingPModeInformation> RetrieveSendingPModeInformation(IEnumerable<string> relatedUserMessageIds)
        {
            var sendingPModes = _repository.RetrieveSendingPModeStringForOutMessages(relatedUserMessageIds);

            var sendingPModeLookup = new Dictionary<string, SendingPModeInformation>();

            foreach (var item in sendingPModes)
            {
                if (sendingPModeLookup.ContainsKey(item.ebmsMessageId) == false)
                {
                    sendingPModeLookup.Add(item.ebmsMessageId, new SendingPModeInformation(item.sendingPMode));
                }
            }
            return sendingPModeLookup;
        }
        
        private static bool IsSignalMessageDuplicate(MessageUnit signalMessage, IDictionary<string, bool> duplicateSignalMessages)
        {
            duplicateSignalMessages.TryGetValue(signalMessage.RefToMessageId, out var isDuplicate);

            if (isDuplicate)
            {
                Logger.Info($"[{signalMessage.RefToMessageId}] Incoming Signal Message is a duplicated one");
            }

            return isDuplicate;
        }

        private void AttemptToInsertSignalMessage(SignalMessage signalMessage, AS4Message as4Message, SendingPModeInformation sendingPMode, string location, CancellationToken token)
        {
            try
            {
                if (signalMessage is Receipt)
                {
                    this.InsertReceipt(signalMessage, as4Message, sendingPMode, location, token);
                }
                else if (signalMessage is Error)
                {
                    this.InsertError(signalMessage, as4Message, sendingPMode, location, token);
                }
            }
            catch (Exception exception)
            {
                ThrowAS4Exception($"Unable to update SignalMessage {signalMessage.MessageId}", as4Message, exception);
            }
        }

        private void InsertReceipt(SignalMessage signalMessage, AS4Message as4Message, SendingPModeInformation sendingPMode, string location, CancellationToken token)
        {
            Logger.Info($"Update Message: {signalMessage.MessageId} as Receipt");
            InMessage inMessage = CreateReceiptInMessage(signalMessage, as4Message, sendingPMode, location, token);

            _repository.InsertInMessage(inMessage);

            UpdateRefUserMessageStatus(signalMessage, OutStatus.Ack);
        }

        private void InsertError(SignalMessage signalMessage, AS4Message as4Message, SendingPModeInformation sendingPMode, string location, CancellationToken cancellationToken)
        {
            Logger.Info($"Update Message: {signalMessage.MessageId} as Error");

            if (Logger.IsWarnEnabled)
            {
                var errorSignalMessage = signalMessage as Error;

                if (errorSignalMessage != null)
                {
                    foreach (var error in errorSignalMessage.Errors)
                    {
                        Logger.Warn($"{error.RefToMessageInError} {error.ErrorCode}: {error.ShortDescription} {error.Detail}");
                    }
                }
            }

            InMessage inMessage = CreateErrorInMessage(signalMessage, as4Message, sendingPMode, location, cancellationToken);

            _repository.InsertInMessage(inMessage);

            UpdateRefUserMessageStatus(signalMessage, OutStatus.Nack);
        }

        private static InMessage CreateReceiptInMessage(
            SignalMessage signalMessage, AS4Message as4Message, SendingPModeInformation sendingPMode, string location, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForSignalMessage(signalMessage, as4Message)
                                                  .WithPModeString(sendingPMode.SendingPModeString)
                                                  .Build(cancellationToken);
            inMessage.MessageLocation = location;

            if (ReceiptDoesNotNeedToBeNotified(sendingPMode.SendingPMode) || signalMessage.IsDuplicated)
            {
                return inMessage;
            }

            AddOperationNotified(inMessage);

            return inMessage;
        }

        private static bool ReceiptDoesNotNeedToBeNotified(SendingProcessingMode sendingPMode)
        {
            return !sendingPMode.ReceiptHandling.NotifyMessageProducer;
        }

        private static InMessage CreateErrorInMessage(
            SignalMessage signalMessage, AS4Message as4Message, SendingPModeInformation sendingPMode, string location, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForSignalMessage(signalMessage, as4Message)
                                                  .WithPModeString(sendingPMode.SendingPModeString)
                                                  .Build(cancellationToken);
            inMessage.MessageLocation = location;

            if (ErrorDontNeedToBeNotified(sendingPMode.SendingPMode) || signalMessage.IsDuplicated)
            {
                return inMessage;
            }

            AddOperationNotified(inMessage);

            return inMessage;
        }

        private static bool ErrorDontNeedToBeNotified(SendingProcessingMode sendingPMode)
        {
            return !sendingPMode.ErrorHandling.NotifyMessageProducer;
        }

        private static void AddOperationNotified(MessageEntity inMessage)
        {
            inMessage.Operation = Operation.ToBeNotified;
        }

        private void UpdateRefUserMessageStatus(MessageUnit signalMessage, OutStatus status)
        {
            if (status == OutStatus.NotApplicable)
            {
                return;
            }

            _repository.UpdateOutMessage(signalMessage.RefToMessageId,
                outMessage => outMessage.Status = status);
        }

        #endregion SignalMessage related

        public async Task UpdateAS4MessageForDelivery(AS4Message as4Message, IAS4MessageBodyPersister as4MessageBodyPersister, CancellationToken cancellationToken)
        {
            var inMessage = _repository.GetInMessageById(as4Message.GetPrimaryMessageId());

            if (inMessage == null)
            {
                throw new InvalidDataException($"Unable to find an InMessage for {as4Message.GetPrimaryMessageId()}");
            }

            await as4MessageBodyPersister.UpdateAS4MessageAsync(inMessage.MessageLocation, as4Message, cancellationToken);

            foreach (var userMessage in as4Message.UserMessages)
            {
                _repository.UpdateInMessage(userMessage.MessageId, message =>
                    {
                        message.PMode = as4Message.GetReceivingPModeString();
                        if (UserMessageNeedsToBeDelivered(as4Message.ReceivingPMode, userMessage))
                        {
                            message.Operation = Operation.ToBeDelivered;
                        }
                    }
                );
            }
        }

        private static bool UserMessageNeedsToBeDelivered(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return pmode.Deliver.IsEnabled && !userMessage.IsDuplicate && !userMessage.IsTest;
        }

        private static void ThrowAS4Exception(string description, AS4Message message, Exception exception)
        {
            Logger.Error(description);

            throw AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(message.MessageIds)
                .WithInnerException(exception)
                .WithReceivingPMode(message.ReceivingPMode)
                .Build();
        }
    }
}