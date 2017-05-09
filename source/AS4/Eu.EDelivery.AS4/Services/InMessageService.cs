using System;
using System.Collections.Generic;
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
        private readonly IAS4MessageBodyPersister _messageBodyPersister;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageService"/> class. 
        /// Create a new Data store Repository
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="as4MessageBodyPersister"></param>
        public InMessageService(IDatastoreRepository repository, IAS4MessageBodyPersister as4MessageBodyPersister)
        {
            _repository = repository;
            _messageBodyPersister = as4MessageBodyPersister;
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

        public async Task InsertAS4Message(AS4Message as4Message, CancellationToken cancellationToken)
        {
            // TODO: should we start the transaction here.
            string location = await _messageBodyPersister.SaveAS4MessageAsync(as4Message, cancellationToken);

            IDictionary<string, bool> duplicateUserMessages =
                this.DetermineDuplicateUserMessageIds(as4Message.UserMessages.Select(m => m.MessageId));

            foreach (UserMessage userMessage in as4Message.UserMessages)
            {
                userMessage.IsTest = IsUserMessageTest(userMessage);
                userMessage.IsDuplicate = IsUserMessageDuplicate(userMessage, duplicateUserMessages);

                AttemptToInsertUserMessage(userMessage, as4Message, location, cancellationToken);
            }

            IDictionary<string, bool> duplicateSignalMessages =
                this.DetermineDuplicateSignalMessageIds(as4Message.SignalMessages.Select(m => m.RefToMessageId));

            foreach (SignalMessage signalMessage in as4Message.SignalMessages)
            {
                signalMessage.IsDuplicated = IsSignalMessageDuplicate(signalMessage, duplicateSignalMessages);
                AttemptToInsertSignalMessage(signalMessage, as4Message, location, cancellationToken);
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
                                                  .WithPModeString(AS4XmlSerializer.ToString(as4Message.ReceivingPMode))
                                                  .Build(cancellationToken);

            inMessage.MessageLocation = messageLocation;

            if (NeedUserMessageBeDelivered(as4Message.ReceivingPMode, userMessage))
            {
                AddOperationDelivered(inMessage);
            }

            return inMessage;
        }

        private static bool NeedUserMessageBeDelivered(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return pmode.Deliver.IsEnabled && !userMessage.IsDuplicate && !userMessage.IsTest;
        }

        private static void AddOperationDelivered(MessageEntity inMessage)
        {
            inMessage.Operation = Operation.ToBeDelivered;
        }

        #endregion

        #region SignalMessage related

        private static bool IsSignalMessageDuplicate(MessageUnit signalMessage, IDictionary<string, bool> duplicateSignalMessages)
        {
            duplicateSignalMessages.TryGetValue(signalMessage.RefToMessageId, out var isDuplicate);

            if (isDuplicate)
            {
                Logger.Info($"[{signalMessage.RefToMessageId}] Incoming Signal Message is a duplicated one");
            }

            return isDuplicate;
        }

        private void AttemptToInsertSignalMessage(SignalMessage signalMessage, AS4Message as4Message, string location, CancellationToken token)
        {
            try
            {
                if (signalMessage is Receipt)
                {
                    this.InsertReceipt(signalMessage, as4Message, location, token);
                }
                else if (signalMessage is Error)
                {
                    this.InsertError(signalMessage, as4Message, location, token);
                }
            }
            catch (Exception exception)
            {
                ThrowAS4Exception($"Unable to update SignalMessage {signalMessage.MessageId}", as4Message, exception);
            }
        }

        private void InsertReceipt(SignalMessage signalMessage, AS4Message as4Message, string location, CancellationToken token)
        {
            Logger.Info($"Update Message: {signalMessage.MessageId} as Receipt");
            InMessage inMessage = CreateReceiptInMessage(signalMessage, as4Message, location, token);

            _repository.InsertInMessage(inMessage);

            UpdateRefUserMessageStatus(signalMessage, OutStatus.Ack);
        }

        private void InsertError(SignalMessage signalMessage, AS4Message as4Message, string location, CancellationToken cancellationToken)
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

            InMessage inMessage = CreateErrorInMessage(signalMessage, as4Message, location, cancellationToken);

            _repository.InsertInMessage(inMessage);

            UpdateRefUserMessageStatus(signalMessage, OutStatus.Nack);
        }

        private static InMessage CreateReceiptInMessage(
            SignalMessage signalMessage, AS4Message as4Message, string location, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForSignalMessage(signalMessage, as4Message)
                                                  .WithPModeString(AS4XmlSerializer.ToString(as4Message.SendingPMode))
                                                  .Build(cancellationToken);
            inMessage.MessageLocation = location;

            if (ReceiptDoesNotNeedToBeNotified(as4Message) || signalMessage.IsDuplicated)
            {
                return inMessage;
            }

            AddOperationNotified(inMessage);

            return inMessage;
        }

        private static bool ReceiptDoesNotNeedToBeNotified(AS4Message as4Message)
        {
            return !as4Message.SendingPMode.ReceiptHandling.NotifyMessageProducer;
        }

        private static InMessage CreateErrorInMessage(
            SignalMessage signalMessage, AS4Message as4Message, string location, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForSignalMessage(signalMessage, as4Message)
                                                  .WithPModeString(AS4XmlSerializer.ToString(as4Message.SendingPMode))
                                                  .Build(cancellationToken);
            inMessage.MessageLocation = location;

            if (ErrorDontNeedToBeNotified(as4Message) || signalMessage.IsDuplicated)
            {
                return inMessage;
            }

            AddOperationNotified(inMessage);

            return inMessage;
        }

        private static bool ErrorDontNeedToBeNotified(AS4Message as4Message)
        {
            return !as4Message.SendingPMode.ErrorHandling.NotifyMessageProducer;
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

    public interface IInMessageService
    {
        Task InsertAS4Message(AS4Message as4Message, CancellationToken cancellationToken);

        /// <summary>
        /// Search for duplicate <see cref="UserMessage"/> instances in the configured datastore for the given <paramref name="searchedMessageIds"/>.
        /// </summary>
        /// <param name="searchedMessageIds">'EbmsMessageIds' to search for duplicates.</param>
        /// <returns></returns>
        IDictionary<string, bool> DetermineDuplicateUserMessageIds(IEnumerable<string> searchedMessageIds);

        /// <summary>
        /// Search for duplicate <see cref="SignalMessage"/> instances in the configured datastore for the given <paramref name="searchedMessageIds"/>.
        /// </summary>
        /// <param name="searchedMessageIds">'RefToEbmsMessageIds' to search for duplicates.</param>
        /// <returns></returns>
        IDictionary<string, bool> DetermineDuplicateSignalMessageIds(IEnumerable<string> searchedMessageIds);
    }
}