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
            this._repository = repository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Insert <see cref="Model.Core.Receipt"/>
        /// into the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="as4Message"></param>
        /// <returns></returns>
        public async Task InsertReceiptAsync(string refToMessageId, AS4Message as4Message)
        {
            await TryInsertOutcomingOutMessageAsync(refToMessageId, as4Message, MessageType.Receipt);
        }

        /// <summary>
        /// Insert <see cref="Model.Core.Error"/>
        /// into the Data store
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="as4Message"></param>
        /// <returns></returns>
        public async Task InsertErrorAsync(string refToMessageId, AS4Message as4Message)
        {
            await TryInsertOutcomingOutMessageAsync(refToMessageId, as4Message, MessageType.Error);
        }

        private async Task TryInsertOutcomingOutMessageAsync(string messageId, AS4Message as4Message, MessageType messageType)
        {
            try
            {
                this._logger.Debug($"Store OutMessage: {messageId}");
                OutMessage outMessage = CreateOutMessage(messageId, as4Message, messageType);
                await this._repository.InsertOutMessageAsync(outMessage);
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

        private static OutMessage CreateOutMessage(string messageId, AS4Message message, MessageType messageType)
        {
            OutMessage outMessage = CreateDefaultOutMessage(messageId, message, messageType);
            AdaptToSignalOutMessage(outMessage);

            Operation operation;
            OutStatus status;

            DetermineCorrectReplyPattern(messageType, message, out operation, out status);

            outMessage.Status = status;
            outMessage.Operation = operation;

            return outMessage;
        }

        private static OutMessage CreateDefaultOutMessage(string messageId, AS4Message as4Message, MessageType messageType)
        {
            return new OutMessageBuilder()
                .WithAS4Message(as4Message)
                .WithEbmsMessageId(messageId)
                .WithEbmsMessageType(messageType)
                .Build(CancellationToken.None);
        }

        private static void AdaptToSignalOutMessage(MessageEntity outMessage)
        {
            outMessage.EbmsRefToMessageId = outMessage.EbmsMessageId;
            outMessage.EbmsMessageId = string.Empty;
        }

        private static void DetermineCorrectReplyPattern(MessageType outMessageType, AS4Message message, out Operation operation, out OutStatus status)
        {
            bool isCallback = outMessageType == MessageType.Error ? IsErrorReplyPatternCallback(message)
                                                                  : IsReceiptReplyPatternCallback(message);

            operation = isCallback ? Operation.ToBeSent : Operation.ToBeNotified;
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
        Task InsertErrorAsync(string messageId, AS4Message as4Message);
        Task InsertReceiptAsync(string refToMessageId, AS4Message as4Message);
    }
}