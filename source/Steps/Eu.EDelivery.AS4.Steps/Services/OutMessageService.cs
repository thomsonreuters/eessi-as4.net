using System;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using Exception = System.Exception;

namespace Eu.EDelivery.AS4.Steps.Services
{
    /// <summary>
    /// Repository to expose Data store related operations
    /// for the Exception Handling Decorator Steps
    /// </summary>
    public class OutMessageService : IOutMessageService
    {
        private readonly ILogger _logger;
        private readonly IDatastoreRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService"/> class. 
        /// Create a new Insert Data store Repository
        /// with a given Data store
        /// </summary>
        /// <param name="repository">
        /// </param>
        public OutMessageService(IDatastoreRepository repository)
        {
            _repository = repository;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Insert <see cref="Model.Core.Receipt"/>
        /// into the Data store
        /// </summary>        
        /// <param name="as4Message"></param>
        /// <returns></returns>
        public void InsertReceipt(AS4Message as4Message)
        {
            // The Primary SignalMessage of the given AS4Message should be a Receipt
            if (!(as4Message.IsSignalMessage && as4Message.PrimarySignalMessage is Receipt))
            {
                throw new ArgumentException(@"The AS4Message should represent a Receipt", nameof(AS4Message));
            }

            TryInsertOutcomingOutMessage(as4Message, MessageType.Receipt);
        }

        /// <summary>
        /// Insert <see cref="Model.Core.Error"/>
        /// into the Data store
        /// </summary>        
        /// <param name="as4Message"></param>
        /// <returns></returns>
        public void InsertError(AS4Message as4Message)
        {
            if (!(as4Message.IsSignalMessage && as4Message.PrimarySignalMessage is Error))
            {
                throw new ArgumentException(@"The AS4Message should represent an Error", nameof(AS4Message));
            }

            TryInsertOutcomingOutMessage(as4Message, MessageType.Error);
        }

        private void TryInsertOutcomingOutMessage(AS4Message as4Message, MessageType messageType)
        {
            try
            {                
                OutMessage outMessage = CreateOutMessageForSignal(as4Message, messageType);
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

        private static OutMessage CreateOutMessageForSignal(AS4Message message, MessageType messageType)
        {
            var primarySignalMessage = message.PrimarySignalMessage;
            
            OutMessage outMessage = new OutMessageBuilder()
                .WithAS4Message(message)
                .WithEbmsMessageId(primarySignalMessage.MessageId)
                .WithEbmsMessageType(messageType)
                .Build(CancellationToken.None);

            outMessage.EbmsRefToMessageId = primarySignalMessage.RefToMessageId;

            Operation operation;
            OutStatus status;

            DetermineCorrectReplyPattern(messageType, message, out operation, out status);

            outMessage.Status = status;
            outMessage.Operation = operation;

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
        void InsertError(AS4Message as4Message);
        void InsertReceipt(AS4Message as4Message);
    }
}