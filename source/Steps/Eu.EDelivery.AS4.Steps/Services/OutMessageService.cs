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
        private AS4Message _as4Message;

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
            this._as4Message = as4Message;
            await TryInsertOutcomingOutMessageAsync(refToMessageId, MessageType.Receipt);
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
            this._as4Message = as4Message;
            await TryInsertOutcomingOutMessageAsync(refToMessageId, MessageType.Error);
        }

        private async Task TryInsertOutcomingOutMessageAsync(string messageId, MessageType messageType)
        {
            try
            {
                this._logger.Debug($"Store OutMessage: {messageId}");
                OutMessage outMessage = CreateOutMessage(messageId, messageType);
                await this._repository.InsertOutMessageAsync(outMessage);
            }
            catch (Exception)
            {
                this._logger.Error("Cannot insert Error OutMessage into the Datastore");
            }
        }

        private OutMessage CreateOutMessage(string messageId, MessageType messageType)
        {
            OutMessage outMessage = CreateDefaultOutMessage(messageId, messageType);
            AdaptToOutMessage(outMessage);
            AssignRightReplyPattern(outMessage);

            return outMessage;
        }

        private OutMessage CreateDefaultOutMessage(string messageId, MessageType messageType)
        {
            return new OutMessageBuilder()
                .WithAS4Message(this._as4Message)
                .WithEbmsMessageId(messageId)
                .WithEbmsMessageType(messageType)
                .Build(CancellationToken.None);
        }

        private void AdaptToOutMessage(OutMessage outMessage)
        {
            outMessage.EbmsRefToMessageId = outMessage.EbmsMessageId;
            outMessage.EbmsMessageId = string.Empty;
        }

        private void AssignRightReplyPattern(OutMessage outMessage)
        {
            bool isCallback = outMessage.EbmsMessageType == MessageType.Error
                ? IsErrorReplyPatternCallback()
                : IsReceiptReplyPatternCallback();

            outMessage.Operation = isCallback ? Operation.ToBeSent : Operation.NotApplicable;
            outMessage.Status = isCallback ? OutStatus.Created : OutStatus.Sent;
        }

        private bool IsErrorReplyPatternCallback()
        {
            return this._as4Message.ReceivingPMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private bool IsReceiptReplyPatternCallback()
        {
            return this._as4Message.ReceivingPMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;
        }
    }

    public interface IOutMessageService
    {
        Task InsertErrorAsync(string messageId, AS4Message as4Message);
        Task InsertReceiptAsync(string refToMessageId, AS4Message as4Message);
    }
}