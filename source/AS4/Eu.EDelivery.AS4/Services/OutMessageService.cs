using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
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
        private readonly IConfig _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService"/> class. 
        /// Create a new Insert Data store Repository
        /// with a given Data store
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="as4MessageBodyPersister">The <see cref="IAS4MessageBodyPersister"/> that must be used to persist the AS4 Message Body.</param>
        public OutMessageService(IDatastoreRepository repository, IAS4MessageBodyPersister as4MessageBodyPersister)
            : this(Config.Instance, repository, as4MessageBodyPersister) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="respository">The respository.</param>
        /// <param name="as4MessageBodyPersister">The as4 message body persister.</param>
        public OutMessageService(IConfig config, IDatastoreRepository respository, IAS4MessageBodyPersister as4MessageBodyPersister)
        {
            _configuration = config;
            _repository = respository;
            _messageBodyPersister = as4MessageBodyPersister;
        }

        /// <summary>
        /// Inserts a s4 message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task InsertAS4Message(AS4Message message, Operation operation, CancellationToken cancellationToken)
        {
            string messageBodyLocation = await _messageBodyPersister.SaveAS4MessageAsync(_configuration.OutStore, message, cancellationToken);
            
            foreach (UserMessage userMessage in message.UserMessages)
            {
                OutMessage outMessage = CreateOutMessageForMessageUnit(userMessage, message, messageBodyLocation, operation);

                _repository.InsertOutMessage(outMessage);
            }

            foreach (SignalMessage signalMessage in message.SignalMessages)
            {
                OutMessage outMessage = CreateOutMessageForMessageUnit(signalMessage, message, messageBodyLocation, operation);

                _repository.InsertOutMessage(outMessage);
            }            
        }
       
        private static OutMessage CreateOutMessageForMessageUnit(MessageUnit messageUnit, AS4Message as4Message, string location, Operation operation)
        {
            OutMessage outMessage = OutMessageBuilder.ForAS4Message(messageUnit, as4Message)
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

                DetermineCorrectReplyPattern(outMessage.EbmsMessageType, as4Message, out determinedOperation, out status);

                outMessage.Status = status;
                outMessage.Operation = determinedOperation;
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