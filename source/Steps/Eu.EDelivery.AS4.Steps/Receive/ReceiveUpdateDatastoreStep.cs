using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is received.
    /// </summary>
    public class ReceiveUpdateDatastoreStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IInMessageService _service;
        private AS4Message _as4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveUpdateDatastoreStep"/> class
        /// </summary>
        public ReceiveUpdateDatastoreStep()
        {
            this._service = new InMessageService(Registry.Instance.DatastoreRepository);
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveUpdateDatastoreStep"/> class
        /// Create a new Update data store operation
        /// for the Receive Operation
        /// </summary>
        /// <param name="service"> </param>
        public ReceiveUpdateDatastoreStep(IInMessageService service)
        {
            this._service = service;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception if the data store cannot be updated</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken token)
        {
            this._logger.Info($"{internalMessage.Prefix} Update Datastore with AS4 received message");
            this._as4Message = internalMessage.AS4Message;

            await UpdateUserMessagesAsync(token);
            await UpdateSignalMessagesAsync(token);

            return StepResult.Success(internalMessage);
        }

        private async Task UpdateUserMessagesAsync(CancellationToken token)
        {
            foreach (UserMessage userMessage in this._as4Message.UserMessages)
                await UpdateUserMessageAsync(userMessage, token);
        }

        private async Task UpdateUserMessageAsync(UserMessage userMessage, CancellationToken token)
        {
            if (IsUserMessageTest(userMessage))
                userMessage.IsTest = true;

            if (IsUserMessageDuplicate(userMessage))
                userMessage.IsDuplicate = true;

            await TryUpdateUserMessageAsync(token, userMessage);
        }

        private bool IsUserMessageTest(UserMessage userMessage)
        {
            CollaborationInfo collaborationInfo = userMessage.CollaborationInfo;

            bool isTestMessage =
                collaborationInfo.Service.Value.Equals(Constants.Namespaces.TestService) &&
                collaborationInfo.Action.Equals(Constants.Namespaces.TestAction);

            if (isTestMessage)
                this._logger.Info($"[{userMessage.MessageId}] Incoming User Message is 'Test Message'");

            return isTestMessage;
        }

        private bool IsUserMessageDuplicate(MessageUnit userMessage)
        {
            bool isDuplicate = this._service.ContainsUserMessageWithId(userMessage.MessageId);

            if(isDuplicate)
                this._logger.Info($"[{userMessage.MessageId}] Incoming User Message is a duplicated one");

            return isDuplicate;
        }

        private async Task TryUpdateUserMessageAsync(CancellationToken token, UserMessage userMessage)
        {
            try
            {
                await this._service.InsertUserMessageAsync(userMessage, this._as4Message, token);
            }
            catch (Exception exception)
            {
                ThrowAS4Exception($"Unable to update UserMessage {userMessage.MessageId}", exception);
            }
        }

        private async Task UpdateSignalMessagesAsync(CancellationToken token)
        {
            foreach (SignalMessage signalMessage in this._as4Message.SignalMessages)
                await UpdateSignalMessageAsync(signalMessage, token);
        }

        private async Task UpdateSignalMessageAsync(SignalMessage signalMessage, CancellationToken token)
        {
            if (IsSignalMessageDuplicate(signalMessage))
                signalMessage.IsDuplicated = true;

            await TryUpdateSignalMessageAsync(signalMessage, token);
        }

        private bool IsSignalMessageDuplicate(SignalMessage signalMessage)
        {
            bool isDuplicate = this._service.ContainsSignalMessageWithReferenceToMessageId(signalMessage.RefToMessageId);

            if(isDuplicate)
                this._logger.Info($"[{signalMessage.RefToMessageId}] Incoming Signal Message is a duplicated one");

            return isDuplicate;
        }

        private async Task TryUpdateSignalMessageAsync(SignalMessage signalMessage, CancellationToken token)
        {
            try
            {
                await UpdateSignalMessage(signalMessage, token);
            }
            catch (Exception exception)
            {
                ThrowAS4Exception($"Unable to update SignalMessage {signalMessage.MessageId}", exception);
            }
        }

        private async Task UpdateSignalMessage(SignalMessage signalMessage, CancellationToken token)
        {
            if (signalMessage is Receipt)
                await UpdateReceipt(signalMessage, token);

            else if (signalMessage is Error)
                await UpdateError(signalMessage, token);
        }

        private async Task UpdateReceipt(SignalMessage signalMessage, CancellationToken cancellationToken)
        {
            await this._service.InsertReceiptAsync(signalMessage, this._as4Message, cancellationToken);

            OutStatus status = IsSignalMessageReferenceUserMessage(signalMessage)
                ? OutStatus.Ack
                : OutStatus.NotApplicable;

            await this._service.UpdateSignalMessage(signalMessage, status, cancellationToken);
        }

        private async Task UpdateError(SignalMessage signalMessage, CancellationToken cancellationToken)
        {
            await this._service.InsertErrorAsync(signalMessage, this._as4Message, cancellationToken);

            OutStatus status = IsSignalMessageReferenceUserMessage(signalMessage)
                ? OutStatus.Nack
                : OutStatus.NotApplicable;

            await this._service.UpdateSignalMessage(signalMessage, status, cancellationToken);
        }

        private bool IsSignalMessageReferenceUserMessage(SignalMessage signalMessage)
        {
            return signalMessage.RefToMessageId.Equals(this._as4Message.PrimaryUserMessage?.MessageId);
        }

        private void ThrowAS4Exception(string description, Exception exception)
        {
            this._logger.Error(description);

            throw new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithMessageIds(this._as4Message.MessageIds)
                .WithInnerException(exception)
                .WithReceivingPMode(this._as4Message.ReceivingPMode)
                .Build();
        }
    }
}