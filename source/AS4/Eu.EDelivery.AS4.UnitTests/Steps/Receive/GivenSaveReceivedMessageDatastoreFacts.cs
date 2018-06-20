using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
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
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        public GivenSaveReceivedMessageDatastoreFacts()
        {
            Step = new SaveReceivedMessageStep(StubConfig.Default, GetDataStoreContext, _messageBodyStore);
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

        public class GivenValidArguments : GivenSaveReceivedMessageDatastoreFacts
        {
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
                    await Step.ExecuteAsync(messagingContext);

                    // Assert
                    InMessage m = await GettUserInMessage(userMessage);
                    Assert.Equal(Operation.NotApplicable, m.Operation.ToEnum<Operation>());
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
                    StepResult stepResult = await Step.ExecuteAsync(messagingContext);
                    Assert.False(stepResult.MessagingContext.AS4Message.PrimarySignalMessage.IsDuplicate);
                }

                using (MessagingContext messagingContext = 
                    CreateReceivedMessagingContext(AS4Message.Create(signalMessage), null))
                {
                    StepResult stepResult = await Step.ExecuteAsync(messagingContext);

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

                using (MessagingContext context = 
                    CreateReceivedMessagingContext(AS4Message.Create(userMessage), pmode))
                {
                    // Act
                    await Step.ExecuteAsync(context);

                    // Assert
                    InMessage m = await GettUserInMessage(userMessage);
                    Assert.Equal(Operation.NotApplicable, m.Operation.ToEnum<Operation>());
                }
            }

            

            private async Task InsertDuplicateUserMessage(MessageUnit userMessage)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    var inMessage = new InMessage(ebmsMessageId: userMessage.MessageId);
                    context.InMessages.Add(inMessage);
                    await context.SaveChangesAsync();
                }
            }

            private async Task<InMessage> GettUserInMessage(MessageUnit userMessage)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InMessage inMessage = await context.InMessages
                                                       .FirstOrDefaultAsync(m => m.EbmsMessageId.Equals(userMessage.MessageId));

                    Assert.NotNull(inMessage);
                    Assert.Equal(MessageType.UserMessage, inMessage.EbmsMessageType.ToEnum<MessageType>());

                    return inMessage;
                }
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                using (MessagingContext context = 
                    CreateReceivedMessagingContext(AS4Message.Empty, receivingPMode: null))
                {
                    // Act
                    StepResult result = await Step.ExecuteAsync(context);

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
            string userMessageId = Guid.NewGuid().ToString();
            return new UserMessage(userMessageId) { RefToMessageId = userMessageId };
        }

        private static void AddTestableDataToUserMessage(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = Constants.Namespaces.TestAction;
            userMessage.CollaborationInfo.Service.Value = Constants.Namespaces.TestService;
        }

        protected MessagingContext CreateReceivedMessagingContext(AS4Message as4Message, ReceivingProcessingMode receivingPMode)
        {
            var stream = new MemoryStream();

            SerializerProvider.Default.Get(as4Message.ContentType).Serialize(as4Message, stream, CancellationToken.None);
            stream.Position = 0;

            var ctx = new MessagingContext(
                new ReceivedMessage(stream, as4Message.ContentType), 
                MessagingContextMode.Receive)
                {ReceivingPMode = receivingPMode};

            ctx.ModifyContext(as4Message);
            return ctx;
        }
    }
}