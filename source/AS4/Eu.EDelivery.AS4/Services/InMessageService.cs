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
using Eu.EDelivery.AS4.Model.Internal;
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

        public async Task InsertAS4Message(
            InternalMessage message,
            IAS4MessageBodyPersister as4MessageBodyPersister,
            CancellationToken cancellationToken)
        {
            AS4Message as4Message = message.AS4Message;
            // TODO: should we start the transaction here.
            string location = await as4MessageBodyPersister.SaveAS4MessageAsync(as4Message, cancellationToken);

            IDictionary<string, bool> duplicateUserMessages =
                this.DetermineDuplicateUserMessageIds(as4Message.UserMessages.Select(m => m.MessageId));

            foreach (UserMessage userMessage in as4Message.UserMessages)
            {
                userMessage.IsTest = IsUserMessageTest(userMessage);
                userMessage.IsDuplicate = IsUserMessageDuplicate(userMessage, duplicateUserMessages);

                AttemptToInsertUserMessage(userMessage, message, location, cancellationToken);
            }

            if (as4Message.SignalMessages.Any())
            {
                var relatedUserMessageIds = as4Message.SignalMessages.Select(m => m.RefToMessageId);

                IDictionary<string, bool> duplicateSignalMessages = this.DetermineDuplicateSignalMessageIds(relatedUserMessageIds);

                foreach (SignalMessage signalMessage in as4Message.SignalMessages)
                {
                    signalMessage.IsDuplicated = IsSignalMessageDuplicate(signalMessage, duplicateSignalMessages);

                    AttemptToInsertSignalMessage(signalMessage, message, location, cancellationToken);
                }
            }
        }

        public async Task UpdateAS4MessageForDeliveryAndNotification(
            InternalMessage internalMessage,
            IAS4MessageBodyPersister as4MessageBodyPersister,
            CancellationToken cancellationToken)
        {
            AS4Message as4Message = internalMessage.AS4Message;
            var inMessage = _repository.GetInMessageById(as4Message.GetPrimaryMessageId());

            if (inMessage == null)
            {
                throw new InvalidDataException($"Unable to find an InMessage for {as4Message.GetPrimaryMessageId()}");
            }

            if (as4Message.UserMessages.Any())
            {
                await as4MessageBodyPersister.UpdateAS4MessageAsync(inMessage.MessageLocation, as4Message, cancellationToken);
            }

            foreach (var userMessage in as4Message.UserMessages)
            {
                _repository.UpdateInMessage(userMessage.MessageId, message =>
                {
                    message.PMode = internalMessage.GetReceivingPModeString();
                    if (UserMessageNeedsToBeDelivered(internalMessage.ReceivingPMode, userMessage))
                    {
                        message.Operation = Operation.ToBeDelivered;
                    }
                }
                );
            }

            // Improvement: I think it will be safer if we retrieve the sending-pmodes of the related usermessages ourselves here
            // instead of relying on the SendingPMode that is available in the AS4Message object (which is set by another Step in the queueu).

            foreach (var signalMessage in as4Message.SignalMessages)
            {
                if (signalMessage is Receipt)
                {
                    if (ReceiptMustBeNotified(internalMessage.SendingPMode) && signalMessage.IsDuplicated == false)
                    {
                        _repository.UpdateInMessage(signalMessage.MessageId, r => r.Operation = Operation.ToBeNotified);
                    }
                    UpdateRefUserMessageStatus(signalMessage, OutStatus.Ack);
                }
                else if (signalMessage is Error)
                {
                    if (ErrorMustBeNotified(internalMessage.SendingPMode) && signalMessage.IsDuplicated == false)
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

        private static bool IsUserMessageDuplicate(MessageUnit userMessage, IDictionary<string, bool> duplicateUserMessages)
        {
            duplicateUserMessages.TryGetValue(userMessage.MessageId, out var isDuplicate);

            if (isDuplicate)
            {
                Logger.Info($"[{userMessage.MessageId}] Incoming User Message is a duplicated one");
            }

            return isDuplicate;
        }

        private void AttemptToInsertUserMessage(UserMessage userMessage, InternalMessage message, string location, CancellationToken cancellationToken)
        {
            try
            {
                InMessage inMessage = CreateUserInMessage(userMessage, message, location, cancellationToken);
                _repository.InsertInMessage(inMessage);
            }
            catch (Exception ex)
            {
                ThrowAS4Exception($"Unable to update UserMessage {userMessage.MessageId}", message, ex);
            }
        }

        private static InMessage CreateUserInMessage(
             UserMessage userMessage, InternalMessage message, string messageLocation, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForUserMessage(userMessage, message.AS4Message)
                                                  .WithPModeString(message.GetReceivingPModeString())
                                                  .Build(cancellationToken);

            inMessage.MessageLocation = messageLocation;


            return inMessage;
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

        private void AttemptToInsertSignalMessage(SignalMessage signalMessage, InternalMessage message, string location, CancellationToken token)
        {
            try
            {
                if (signalMessage is Receipt)
                {
                    this.InsertReceipt(signalMessage, message, location, token);
                }
                else if (signalMessage is Error)
                {
                    this.InsertError(signalMessage, message, location, token);
                }
            }
            catch (Exception exception)
            {
                ThrowAS4Exception($"Unable to update SignalMessage {signalMessage.MessageId}", message, exception);
            }
        }

        private void InsertReceipt(SignalMessage signalMessage, InternalMessage message, string location, CancellationToken token)
        {
            Logger.Info($"Update Message: {signalMessage.MessageId} as Receipt");
            InMessage inMessage = CreateReceiptInMessage(signalMessage, message, location, token);

            _repository.InsertInMessage(inMessage);
        }

        private void InsertError(SignalMessage signalMessage, InternalMessage message, string location, CancellationToken cancellationToken)
        {
            Logger.Info($"Update Message: {signalMessage.MessageId} as Error");

            if (Logger.IsWarnEnabled)
            {
                Logger.Warn("Details of the received Error:");

                var errorSignalMessage = signalMessage as Error;

                if (errorSignalMessage != null)
                {
                    foreach (var error in errorSignalMessage.Errors)
                    {
                        Logger.Warn($"{error.RefToMessageInError} {error.ErrorCode}: {error.ShortDescription} {error.Detail}");
                    }
                }
            }

            InMessage inMessage = CreateErrorInMessage(signalMessage, message, location, cancellationToken);

            _repository.InsertInMessage(inMessage);
        }

        private static InMessage CreateReceiptInMessage(
            SignalMessage signalMessage, InternalMessage message, string location, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForSignalMessage(signalMessage, message.AS4Message)
                                                  .WithPModeString(AS4XmlSerializer.ToString(message.SendingPMode))
                                                  .Build(cancellationToken);
            inMessage.MessageLocation = location;

            return inMessage;
        }

        private static bool ReceiptMustBeNotified(SendingProcessingMode sendingPMode)
        {
            return sendingPMode.ReceiptHandling.NotifyMessageProducer;
        }

        private static InMessage CreateErrorInMessage(
            SignalMessage signalMessage, InternalMessage message, string location, CancellationToken cancellationToken)
        {
            InMessage inMessage = InMessageBuilder.ForSignalMessage(signalMessage, message.AS4Message)
                                                  .WithPModeString(AS4XmlSerializer.ToString(message.SendingPMode))
                                                  .Build(cancellationToken);
            inMessage.MessageLocation = location;

            return inMessage;
        }

        private static bool ErrorMustBeNotified(SendingProcessingMode sendingPMode)
        {
            return sendingPMode.ErrorHandling.NotifyMessageProducer;
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

        private static bool UserMessageNeedsToBeDelivered(ReceivingProcessingMode pmode, UserMessage userMessage)
        {
            return pmode.Deliver.IsEnabled && !userMessage.IsDuplicate && !userMessage.IsTest;
        }

        private static void ThrowAS4Exception(string description, InternalMessage message, Exception exception)
        {
            Logger.Error(description);

            throw AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(message.AS4Message.MessageIds)
                .WithInnerException(exception)
                .WithReceivingPMode(message.ReceivingPMode)
                .Build();
        }
    }
}