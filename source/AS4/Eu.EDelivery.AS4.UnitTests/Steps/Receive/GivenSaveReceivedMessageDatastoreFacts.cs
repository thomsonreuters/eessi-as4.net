using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="SaveReceivedMessageStep" />
    /// </summary>
    public class GivenSaveReceivedMessageDatastoreFacts : GivenDatastoreStepFacts
    {
        private readonly string _userMessageId;
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        public GivenSaveReceivedMessageDatastoreFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Default);

            _userMessageId = Guid.NewGuid().ToString();

            Step = new SaveReceivedMessageStep(GetDataStoreContext, _messageBodyStore);
        }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        /// <summary>
        /// Testing the Step with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenSaveReceivedMessageDatastoreFacts
        {
            private static void AddTestableDataToUserMessage(UserMessage userMessage)
            {
                userMessage.CollaborationInfo.Action = Constants.Namespaces.TestAction;
                userMessage.CollaborationInfo.Service.Value = Constants.Namespaces.TestService;
            }

            private async Task InsertDuplicateUserMessage(MessageUnit userMessage)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    var inMessage = new InMessage { EbmsMessageId = userMessage.MessageId };
                    context.InMessages.Add(inMessage);
                    await context.SaveChangesAsync();
                }
            }

            private async Task AssertUserInMessageAsync(MessageUnit userMessage, Func<InMessage, bool> condition = null)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InMessage inMessage = await context.InMessages
                        .FirstOrDefaultAsync(m => m.EbmsMessageId.Equals(userMessage.MessageId));

                    Assert.NotNull(inMessage);
                    Assert.Equal(MessageType.UserMessage, MessageTypeUtils.Parse(inMessage.EbmsMessageType));

                    if (condition != null)
                    {
                        Assert.True(condition(inMessage));
                    }
                }
            }

            [Fact]
            public async Task ThenExecuteStepIsTestUserMessageAsync()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                AddTestableDataToUserMessage(userMessage);

                var as4Message = AS4Message.Create(userMessage);

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;

                using (var messagingContext = CreateReceivedMessagingContext(as4Message, pmode))
                {
                    // Act
                    await Step.ExecuteAsync(messagingContext, CancellationToken.None);

                    // Assert
                    await AssertUserInMessageAsync(userMessage, m => OperationUtils.Parse(m.Operation) == Operation.NotApplicable);
                }
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateReceiptMessageAsync()
            {
                // Arrange
                SignalMessage signalMessage = new Receipt("message-id") { RefToMessageId = "ref-to-message-id" };
                signalMessage.IsDuplicate = false;

                using (var messagingContext = CreateReceivedMessagingContext(AS4Message.Create(signalMessage), null))
                {
                    // Act           
                    // Execute the step twice.     
                    StepResult stepResult = await Step.ExecuteAsync(messagingContext, CancellationToken.None);
                    Assert.False(stepResult.MessagingContext.AS4Message.PrimarySignalMessage.IsDuplicate);
                }

                using (var messagingContext = CreateReceivedMessagingContext(AS4Message.Create(signalMessage), null))
                {
                    StepResult stepResult = await Step.ExecuteAsync(messagingContext, CancellationToken.None);

                    // Assert
                    Assert.True(stepResult.MessagingContext.AS4Message.PrimarySignalMessage.IsDuplicate);
                }
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateUserMessageAsync()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                await InsertDuplicateUserMessage(userMessage);

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;

                using (var context = CreateReceivedMessagingContext(AS4Message.Create(userMessage), pmode))
                {
                    // Act
                    await Step.ExecuteAsync(context, CancellationToken.None);

                    // Assert
                    await AssertUserInMessageAsync(userMessage, m => OperationUtils.Parse(m.Operation) == Operation.NotApplicable);
                }
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                using (var context = CreateReceivedMessagingContext(AS4Message.Empty, null))
                {
                    // Act
                    StepResult result = await Step.ExecuteAsync(context, CancellationToken.None);

                    // Assert
                    Assert.NotNull(result);
                }
            }
        }

        /// <summary>
        /// Create a <see cref="UserMessage" /> based on the configured Id's.
        /// </summary>
        /// <returns></returns>
        protected UserMessage CreateUserMessage()
        {
            return new UserMessage(_userMessageId) { RefToMessageId = _userMessageId };
        }

        protected MessagingContext CreateReceivedMessagingContext(AS4Message as4Message, ReceivingProcessingMode receivingPMode)
        {
            MemoryStream stream = new MemoryStream();

            SerializerProvider.Default.Get(as4Message.ContentType).Serialize(as4Message, stream, CancellationToken.None);
            stream.Position = 0;

            var receivedMessage = new ReceivedMessage(stream, as4Message.ContentType);

            var messagingContext = new MessagingContext(receivedMessage, MessagingContextMode.Receive);
            messagingContext.ReceivingPMode = receivingPMode;

            return messagingContext;
        }
    }
}