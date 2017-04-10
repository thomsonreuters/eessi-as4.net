using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is received.
    /// </summary>
    public class ReceiveUpdateDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private AS4Message _as4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveUpdateDatastoreStep" /> class
        /// </summary>
        public ReceiveUpdateDatastoreStep() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveUpdateDatastoreStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        public ReceiveUpdateDatastoreStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
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
            Logger.Info($"{internalMessage.Prefix} Update Datastore with AS4 received message");
            _as4Message = internalMessage.AS4Message;

            using (DatastoreContext context = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                var service = new InMessageService(repository);

                await UpdateUserMessagesAsync(service, token);
                await UpdateSignalMessagesAsync(service, token);
            }

            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task UpdateUserMessagesAsync(InMessageService service, CancellationToken token)
        {
            foreach (UserMessage userMessage in _as4Message.UserMessages)
            {
                await UpdateUserMessageAsync(userMessage, service, token);
            }
        }

        private async Task UpdateUserMessageAsync(UserMessage userMessage, InMessageService service, CancellationToken token)
        {
            if (IsUserMessageTest(userMessage))
            {
                userMessage.IsTest = true;
            }

            if (IsUserMessageDuplicate(userMessage, service))
            {
                userMessage.IsDuplicate = true;
            }

            await TryUpdateUserMessageAsync(token, userMessage, service);
        }

        private bool IsUserMessageTest(UserMessage userMessage)
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

        private bool IsUserMessageDuplicate(MessageUnit userMessage, InMessageService service)
        {
            bool isDuplicate = service.ContainsUserMessageWithId(userMessage.MessageId);

            if (isDuplicate)
            {
                Logger.Info($"[{userMessage.MessageId}] Incoming User Message is a duplicated one");
            }

            return isDuplicate;
        }

        private async Task TryUpdateUserMessageAsync(CancellationToken token, UserMessage userMessage, InMessageService service)
        {
            try
            {
                await service.InsertUserMessageAsync(userMessage, _as4Message, token);
            }
            catch (Exception exception)
            {
                ThrowAS4Exception($"Unable to update UserMessage {userMessage.MessageId}", exception);
            }
        }

        private async Task UpdateSignalMessagesAsync(InMessageService service, CancellationToken token)
        {
            foreach (SignalMessage signalMessage in _as4Message.SignalMessages)
            {
                await UpdateSignalMessageAsync(signalMessage, service, token);
            }
        }

        private async Task UpdateSignalMessageAsync(SignalMessage signalMessage, InMessageService service, CancellationToken token)
        {
            if (IsSignalMessageDuplicate(signalMessage, service))
            {
                signalMessage.IsDuplicated = true;
            }

            await TryUpdateSignalMessageAsync(signalMessage, service, token);
        }

        private bool IsSignalMessageDuplicate(SignalMessage signalMessage, InMessageService service)
        {
            bool isDuplicate = service.ContainsSignalMessageWithReferenceToMessageId(signalMessage.RefToMessageId);

            if (isDuplicate)
            {
                Logger.Info($"[{signalMessage.RefToMessageId}] Incoming Signal Message is a duplicated one");
            }

            return isDuplicate;
        }

        private async Task TryUpdateSignalMessageAsync(SignalMessage signalMessage, InMessageService service, CancellationToken token)
        {
            try
            {
                await UpdateSignalMessage(signalMessage, service, token);
            }
            catch (Exception exception)
            {
                ThrowAS4Exception($"Unable to update SignalMessage {signalMessage.MessageId}", exception);
            }
        }

        private async Task UpdateSignalMessage(SignalMessage signalMessage, InMessageService service, CancellationToken token)
        {
            if (signalMessage is Receipt)
            {
                await UpdateReceipt(signalMessage, service, token);
            }
            else if (signalMessage is Error)
            {
                await UpdateError(signalMessage, service, token);
            }
        }

        private async Task UpdateReceipt(SignalMessage signalMessage, InMessageService service, CancellationToken cancellationToken)
        {
            await service.InsertReceiptAsync(signalMessage, _as4Message, cancellationToken);

            // Since we've received a Receipt, make sure that the Status of the UserMessage related to this receipt is set to Ack.            
            await service.UpdateSignalMessage(signalMessage, OutStatus.Ack, cancellationToken);
        }

        private async Task UpdateError(SignalMessage signalMessage, InMessageService service, CancellationToken cancellationToken)
        {
            await service.InsertErrorAsync(signalMessage, _as4Message, cancellationToken);

            // Make sure the status of the related UserMessage of this Error is set to Nack.            
            await service.UpdateSignalMessage(signalMessage, OutStatus.Nack, cancellationToken);
        }

        private void ThrowAS4Exception(string description, Exception exception)
        {
            Logger.Error(description);

            throw AS4ExceptionBuilder.WithDescription(description)
                                     .WithMessageIds(_as4Message.MessageIds)
                                     .WithInnerException(exception)
                                     .WithReceivingPMode(_as4Message.ReceivingPMode)
                                     .Build();
        }
    }
}