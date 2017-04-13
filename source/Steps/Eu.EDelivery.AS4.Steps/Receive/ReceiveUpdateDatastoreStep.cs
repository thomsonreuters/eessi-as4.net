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

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveUpdateDatastoreStep" /> class
        /// </summary>
        public ReceiveUpdateDatastoreStep() : this(Registry.Instance.CreateDatastoreContext) { }

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

            using (DatastoreContext context = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new InMessageService(repository);
                var messageUpdate = new ReceiveMessageUpdate(internalMessage, service, token);

                messageUpdate.UpdateUserMessages();
                messageUpdate.UpdateSignalMessages();

                await context.SaveChangesAsync(token);
            }

            return await StepResult.SuccessAsync(internalMessage);
        }

        /// <summary>
        /// Method Object to update the ebMS message instances in the 'Receive' operation.
        /// </summary>
        private class ReceiveMessageUpdate
        {
            private readonly InternalMessage _originalMessage;
            private readonly IInMessageService _messageService;
            private readonly CancellationToken _cancellation;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReceiveMessageUpdate"/> class.
            /// </summary>
            /// <param name="originalMessage">The original Message.</param>
            /// <param name="messageService">The message Service.</param>
            /// <param name="cancellation">The cancellation.</param>
            public ReceiveMessageUpdate(
                InternalMessage originalMessage,
                IInMessageService messageService,
                CancellationToken cancellation)
            {
                _originalMessage = originalMessage;
                _messageService = messageService;
                _cancellation = cancellation;
            }

            /// <summary>
            /// Update the <see cref="UserMessage"/> instances.
            /// </summary>
            public void UpdateUserMessages()
            {
                foreach (UserMessage userMessage in _originalMessage.AS4Message.UserMessages)
                {
                    userMessage.IsTest = IsUserMessageTest(userMessage);
                    userMessage.IsDuplicate = IsUserMessageDuplicate(userMessage);

                    TryUpdateUserMessage(userMessage);
                }
            }

            private static bool IsUserMessageTest(UserMessage userMessage)
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

            private bool IsUserMessageDuplicate(MessageUnit userMessage)
            {
                bool isDuplicate = _messageService.ContainsUserMessageWithId(userMessage.MessageId);

                if (isDuplicate)
                {
                    Logger.Info($"[{userMessage.MessageId}] Incoming User Message is a duplicated one");
                }

                return isDuplicate;
            }

            private void TryUpdateUserMessage(UserMessage userMessage)
            {
                try
                {
                    _messageService.InsertUserMessage(userMessage, _originalMessage.AS4Message, _cancellation);
                }
                catch (Exception exception)
                {
                    ThrowAS4Exception($"Unable to update UserMessage {userMessage.MessageId}", exception);
                }
            }

            /// <summary>
            /// Update the <see cref="SignalMessage"/> instances.
            /// </summary>
            public void UpdateSignalMessages()
            {
                foreach (SignalMessage signalMessage in _originalMessage.AS4Message.SignalMessages)
                {
                    signalMessage.IsDuplicated = IsSignalMessageDuplicate(signalMessage);
                    TryUpdateSignalMessage(signalMessage);
                }
            }

            private bool IsSignalMessageDuplicate(SignalMessage signalMessage)
            {
                bool isDuplicate = _messageService.ContainsSignalMessageWithReferenceToMessageId(signalMessage.RefToMessageId);

                if (isDuplicate)
                {
                    Logger.Info($"[{signalMessage.RefToMessageId}] Incoming Signal Message is a duplicated one");
                }

                return isDuplicate;
            }

            private void TryUpdateSignalMessage(SignalMessage signalMessage)
            {
                try
                {
                    if (signalMessage is Receipt)
                    {
                        UpdateReceipt(signalMessage);
                    }
                    else if (signalMessage is Error)
                    {
                        UpdateError(signalMessage);
                    }
                }
                catch (Exception exception)
                {
                    ThrowAS4Exception($"Unable to update SignalMessage {signalMessage.MessageId}", exception);
                }
            }
            private void UpdateReceipt(SignalMessage signalMessage)

            {
                _messageService.InsertReceipt(signalMessage, _originalMessage.AS4Message, _cancellation);

                // Since we've received a Receipt, make sure that the Status of the UserMessage related to this receipt is set to Ack.            
                _messageService.UpdateSignalMessage(signalMessage, OutStatus.Ack, _cancellation);
            }

            private void UpdateError(SignalMessage signalMessage)
            {
                _messageService.InsertError(signalMessage, _originalMessage.AS4Message, _cancellation);

                // Make sure the status of the related UserMessage of this Error is set to Nack.            
                _messageService.UpdateSignalMessage(signalMessage, OutStatus.Nack, _cancellation);
            }

            private void ThrowAS4Exception(string description, Exception exception)
            {
                Logger.Error(description);

                throw AS4ExceptionBuilder
                    .WithDescription(description)
                    .WithMessageIds(_originalMessage.AS4Message.MessageIds)
                    .WithInnerException(exception)
                    .WithReceivingPMode(_originalMessage.AS4Message.ReceivingPMode)
                    .Build();
            }
        }
    }
}