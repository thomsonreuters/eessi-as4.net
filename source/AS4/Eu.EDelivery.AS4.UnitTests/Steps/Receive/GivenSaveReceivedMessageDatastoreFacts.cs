using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
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

        public GivenSaveReceivedMessageDatastoreFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);

            _userMessageId = Guid.NewGuid().ToString();

            Step = new SaveReceivedMessageStep(GetDataStoreContext, StubMessageBodyStore.Default);
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
                    Assert.Equal(MessageType.UserMessage, inMessage.EbmsMessageType);

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
                MessagingContext messagingContext = new InternalMessageBuilder().WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;
                messagingContext.ReceivingPMode = pmode;

                // Act
                await Step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.NotApplicable);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateReceiptMessageAsync()
            {
                // Arrange
                SignalMessage signalMessage = new Receipt("message-id") { RefToMessageId = "ref-to-message-id" };
                signalMessage.IsDuplicated = false;

                MessagingContext messagingContext = new InternalMessageBuilder().WithSignalMessage(signalMessage).Build();

                // Act           
                // Execute the step twice.     
                StepResult stepResult = await Step.ExecuteAsync(messagingContext, CancellationToken.None);
                Assert.False(stepResult.MessagingContext.AS4Message.PrimarySignalMessage.IsDuplicated);

                stepResult = await Step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                Assert.True(stepResult.MessagingContext.AS4Message.PrimarySignalMessage.IsDuplicated);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateUserMessageAsync()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                await InsertDuplicateUserMessage(userMessage);
                MessagingContext messagingContext = new InternalMessageBuilder().WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;
                messagingContext.ReceivingPMode = pmode;

                // Act
                await Step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.NotApplicable);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                var internalMessage = new MessagingContext(AS4Message.Empty);

                // Act
                StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
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
    }
}