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
        /// Get a UserMessage by a given <paramref name="messageId"/>
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public bool ContainsUserMessageWithId(string messageId)
        {
            _logger.Debug($"Find UserMessage for EbmsMessageId: {messageId}");
            return _repository.InMessageExists(m => m.EbmsMessageId.Equals(messageId));
        }

        /// <summary>
        /// Get SignalMessage by a given <paramref name="refToMessageId"/>
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <returns></returns>
        public bool ContainsSignalMessageWithReferenceToMessageId(string refToMessageId)
        {
            _logger.Debug($"Find SignalMessage for RefToEbmsMessageId: {refToMessageId}");
            return _repository.InMessageExists(m => m.EbmsRefToMessageId == refToMessageId);
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

        /// <summary>
        /// Explicit Update a Signal Message
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <param name="status"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public void UpdateSignalMessage(SignalMessage signalMessage, OutStatus status, CancellationToken cancellationToken)
        {
            if (status == OutStatus.NotApplicable)
            {
                return;
            }

            _repository.UpdateOutMessage(signalMessage.RefToMessageId, outMessage => outMessage.Status = status);
        }
    }

    public interface IInMessageService
    {
        bool ContainsUserMessageWithId(string messageId);
        bool ContainsSignalMessageWithReferenceToMessageId(string refToMessageId);

        void InsertUserMessage(UserMessage usermessage, AS4Message as4Message, CancellationToken cancellationToken);
        void InsertReceipt(SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken);
        void InsertError(SignalMessage signalMessage, AS4Message as4Message, CancellationToken cancellationToken);

        void UpdateSignalMessage(SignalMessage signalMessage, OutStatus status, CancellationToken cancellationToken);
    }
}