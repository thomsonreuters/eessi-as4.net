using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Services
{
    /// <summary>
    /// Repository to expose Data store related operations
    /// for the Update Data store Steps
    /// </summary>
    public class InMessageService : IInMessageService
    {
        private readonly IDatastoreRepository _repository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMessageService"/> class. 
        /// Create a new Data store Repository
        /// </summary>
        /// <param name="respository">
        /// </param>
        public InMessageService(IDatastoreRepository respository)
        {
            _repository = respository;
            _logger = LogManager.GetCurrentClassLogger();
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

        /// <summary>
        /// Update a given User Message in the Data store
        /// </summary>
        /// <param name="usermessage"></param>
        /// <param name="as4Message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void InsertUserMessage(
            UserMessage usermessage, AS4Message as4Message, CancellationToken cancellationToken)
        {
            _logger.Info($"Update Message: {usermessage.MessageId} as User Message");
            InMessage inMessage = CreateUserInMessage(usermessage, as4Message, cancellationToken);

            _repository.InsertInMessage(inMessage);
        }

        private static InMessage CreateUserInMessage(
            UserMessage userMessage, AS4Message as4Message, CancellationToken cancellationToken)
        {
            InMessage inMessage = new InMessageBuilder()
                .WithAS4Message(as4Message)
                .WithEbmsMessageType(MessageType.UserMessage)
                .WithMessageUnit(userMessage)
                .WithPModeString(AS4XmlSerializer.ToString(as4Message.ReceivingPMode))
                .Build(cancellationToken);

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

        /// <summary>
        /// Update a given Receipt Signal Message in the Data store
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <param name="as4Message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void InsertReceipt(SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken)
        {
            _logger.Info($"Update Message: {signalMessage.MessageId} as Receipt");
            InMessage inMessage = CreateReceiptInMessage(signalMessage, as4Message, cancellationToken);

            _repository.InsertInMessage(inMessage);

            UpdateRefUserMessageStatus(signalMessage, OutStatus.Ack);
        }

        private static InMessage CreateReceiptInMessage(
            SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken)
        {
            InMessage inMessage = new InMessageBuilder()
                .WithAS4Message(as4Message)
                .WithEbmsMessageType(MessageType.Receipt)
                .WithMessageUnit(signalMessage)
                .WithPModeString(AS4XmlSerializer.ToString(as4Message.SendingPMode))
                .Build(cancellationToken);

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

        /// <summary>
        /// Update a given Error Signal Message in the Data store
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <param name="as4Message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void InsertError(SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken)
        {
            _logger.Info($"Update Message: {signalMessage.MessageId} as Error");

            if (_logger.IsWarnEnabled)
            {
                var errorSignalMessage = signalMessage as Error;

                if (errorSignalMessage != null)
                {
                    foreach (var error in errorSignalMessage.Errors)
                    {
                        _logger.Warn($"{error.RefToMessageInError} {error.ErrorCode}: {error.ShortDescription} {error.Detail}");
                    }
                }
            }

            InMessage inMessage = CreateErrorInMessage(signalMessage, as4Message, cancellationToken);

            _repository.InsertInMessage(inMessage);

            UpdateRefUserMessageStatus(signalMessage, OutStatus.Nack);
        }

        private static InMessage CreateErrorInMessage(
            SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken)
        {
            InMessage inMessage = new InMessageBuilder()
                .WithAS4Message(as4Message)
                .WithEbmsMessageType(MessageType.Error)
                .WithMessageUnit(signalMessage)
                .WithPModeString(AS4XmlSerializer.ToString(as4Message.SendingPMode))
                .Build(cancellationToken);

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
    }

    public interface IInMessageService
    {
        void InsertUserMessage(UserMessage usermessage, AS4Message as4Message, CancellationToken cancellationToken);
        void InsertReceipt(SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken);
        void InsertError(SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken);

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