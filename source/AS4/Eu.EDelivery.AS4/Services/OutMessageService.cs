using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using Exception = System.Exception;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Repository to expose Data store related operations
    /// for the Exception Handling Decorator Steps
    /// </summary>
    public class OutMessageService : IOutMessageService
    {
        private readonly ILogger _logger;
        private readonly IDatastoreRepository _repository;
        private readonly IAS4MessageBodyPersister _messageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService"/> class. 
        /// Create a new Insert Data store Repository
        /// with a given Data store
        /// </summary>
        /// <param name="repository">
        /// </param>
        /// <param name="as4MessageBodyPersister">The <see cref="IAS4MessageBodyPersister"/> that must be used to persist the AS4 Message Body.</param>
        public OutMessageService(IDatastoreRepository repository, IAS4MessageBodyPersister as4MessageBodyPersister)
        {
            _repository = repository;
            _messageBodyPersister = as4MessageBodyPersister;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task InsertAS4Message(AS4Message message, Operation operation, CancellationToken cancellationToken)
        {
            string location = await _messageBodyPersister.SaveAS4MessageAsync(message, cancellationToken);

            foreach (var userMessage in message.UserMessages)
            {
                TryInsertOutcomingOutMessage(userMessage, message, MessageType.UserMessage, location, operation);
            }

            foreach (var signalMessage in message.SignalMessages)
            {
                MessageType type = DetermineSignalMessageType(signalMessage);
                TryInsertOutcomingOutMessage(signalMessage, message, type, location, operation);
            }

        }

        private static MessageType DetermineSignalMessageType(SignalMessage signalMessage)
        {
            // TODO: this logic should be moved to the outmessagebuilder.
            if (signalMessage is Receipt)
            {
                return MessageType.Receipt;
            }
            if (signalMessage is Error)
            {
                return MessageType.Error;
            }
            throw new ArgumentOutOfRangeException();
        }

        private void TryInsertOutcomingOutMessage(MessageUnit message, AS4Message as4Message, MessageType messageType, string location, Operation operation)
        {
            try
            {
                OutMessage outMessage = CreateOutMessageForMessageUnit(message, messageType, as4Message);
                outMessage.MessageLocation = location;
                outMessage.Operation = operation;
                _repository.InsertOutMessage(outMessage);
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot insert Error OutMessage into the Datastore: {ex.Message}");

                if (ex.InnerException != null)
                {
                    _logger.Error(ex.InnerException.Message);
                }
            }
        }

        private static OutMessage CreateOutMessageForMessageUnit(MessageUnit messageUnit, MessageType messageType, AS4Message as4Message)
        {
            OutMessage outMessage = OutMessageBuilder.ForAS4Message(messageUnit, as4Message)
                                                     .WithEbmsMessageType(messageType)
                                                     .Build(CancellationToken.None);

            if (messageType != MessageType.UserMessage)
            {
                Operation operation;
                OutStatus status;

                DetermineCorrectReplyPattern(messageType, as4Message, out operation, out status);

                outMessage.Status = status;
                outMessage.Operation = operation;
            }

            return outMessage;
        }

        private static void DetermineCorrectReplyPattern(MessageType outMessageType, AS4Message message, out Operation operation, out OutStatus status)
        {
            bool isCallback = outMessageType == MessageType.Error ? IsErrorReplyPatternCallback(message)
                                                                  : IsReceiptReplyPatternCallback(message);

            operation = isCallback ? Operation.ToBeSent : Operation.NotApplicable;
            status = isCallback ? OutStatus.Created : OutStatus.Sent;
        }

        private static bool IsErrorReplyPatternCallback(AS4Message as4Message)
        {
            return as4Message.ReceivingPMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private static bool IsReceiptReplyPatternCallback(AS4Message as4Message)
        {
            return as4Message.ReceivingPMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;
        }
    }

    public interface IOutMessageService
    {
        Task InsertAS4Message(AS4Message message, Operation operation, CancellationToken cancellationToken);
    }
}