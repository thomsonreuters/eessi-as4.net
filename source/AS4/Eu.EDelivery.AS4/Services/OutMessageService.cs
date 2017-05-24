using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Repository to expose Data store related operations
    /// for the Exception Handling Decorator Steps
    /// </summary>
    public class OutMessageService : IOutMessageService
    {        
        private readonly IDatastoreRepository _repository;
        private readonly IAS4MessageBodyPersister _messageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService"/> class. 
        /// Create a new Insert Data store Repository
        /// with a given Data store
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="as4MessageBodyPersister">The <see cref="IAS4MessageBodyPersister"/> that must be used to persist the AS4 Message Body.</param>
        public OutMessageService(IDatastoreRepository repository, IAS4MessageBodyPersister as4MessageBodyPersister)
        {
            _repository = repository;
            _messageBodyPersister = as4MessageBodyPersister;            
        }

        public async Task InsertAS4Message(InternalMessage internalMessage, Operation operation, CancellationToken cancellation)
        {
            AS4Message message = internalMessage.AS4Message;
            string messageBodyLocation = await _messageBodyPersister.SaveAS4MessageAsync(message, cancellation);

            foreach (var userMessage in message.UserMessages)
            {
                OutMessage outMessage = CreateOutMessageForMessageUnit(userMessage, internalMessage, messageBodyLocation, operation);

                _repository.InsertOutMessage(outMessage);
            }

            foreach (var signalMessage in message.SignalMessages)
            {
                OutMessage outMessage = CreateOutMessageForMessageUnit(signalMessage, internalMessage, messageBodyLocation, operation);

                _repository.InsertOutMessage(outMessage);
            }
        }

        private static OutMessage CreateOutMessageForMessageUnit(MessageUnit messageUnit, InternalMessage message, string location, Operation operation)
        {
            OutMessage outMessage = OutMessageBuilder.ForInternalMessage(messageUnit, message)
                                                     .Build(CancellationToken.None);

            outMessage.MessageLocation = location;

            if (outMessage.EbmsMessageType == MessageType.UserMessage)
            {
                outMessage.Operation = operation;
            }
            else
            {
                Operation determinedOperation;
                OutStatus status;

                DetermineCorrectReplyPattern(outMessage.EbmsMessageType, message, out determinedOperation, out status);

                outMessage.Status = status;
                outMessage.Operation = determinedOperation;
            }

            return outMessage;
        }

        private static void DetermineCorrectReplyPattern(MessageType outMessageType, InternalMessage message, out Operation operation, out OutStatus status)
        {
            bool isCallback = outMessageType == MessageType.Error ? IsErrorReplyPatternCallback(message)
                                                                  : IsReceiptReplyPatternCallback(message);

            operation = isCallback ? Operation.ToBeSent : Operation.NotApplicable;
            status = isCallback ? OutStatus.Created : OutStatus.Sent;
        }

        private static bool IsErrorReplyPatternCallback(InternalMessage message)
        {
            return message.ReceivingPMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private static bool IsReceiptReplyPatternCallback(InternalMessage message)
        {
            return message.ReceivingPMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;
        }
    }

    public interface IOutMessageService
    {
        Task InsertAS4Message(InternalMessage message, Operation operation, CancellationToken cancellation);
    }
}