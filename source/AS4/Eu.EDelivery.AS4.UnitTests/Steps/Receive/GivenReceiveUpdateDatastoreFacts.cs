using System;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="ReceiveUpdateDatastoreStep" />
    /// </summary>
    public class GivenReceiveUpdateDatastoreFacts : GivenDatastoreStepFacts
    {
        private readonly string _userMessageId;

        public GivenReceiveUpdateDatastoreFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);

            _userMessageId = Guid.NewGuid().ToString();

            Step = new ReceiveUpdateDatastoreStep();
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        /// <summary>
        /// Testing the Step with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenReceiveUpdateDatastoreFacts
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
                    var inMessage = new InMessage {EbmsMessageId = userMessage.MessageId};
                    context.InMessages.Add(inMessage);
                    await context.SaveChangesAsync();
                }
            }

            private async Task AssertUserInMessageAsync(MessageUnit userMessage, Func<InMessage, bool> condition = null)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InMessage inMessage =
                        await context.InMessages.FirstOrDefaultAsync(m => m.EbmsMessageId.Equals(userMessage.MessageId));

                    Assert.NotNull(inMessage);
                    Assert.Equal(MessageType.UserMessage, inMessage.EbmsMessageType);
                    if (condition != null) Assert.True(condition(inMessage));
                }
            }

            [Fact]
            public async Task ThenExecuteStepIsTestUserMessageAsync()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                AddTestableDataToUserMessage(userMessage);
                InternalMessage internalMessage = new InternalMessageBuilder().WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;
                internalMessage.AS4Message.ReceivingPMode = pmode;

                // Act
                await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.NotApplicable);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithDeliveringAsync()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                InternalMessage internalMessage = new InternalMessageBuilder().WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = false;
                pmode.Deliver.IsEnabled = true;
                internalMessage.AS4Message.ReceivingPMode = pmode;

                // Act
                await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.ToBeDelivered);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateReceiptMessageAsync()
            {
                // Arrange
                SignalMessage signalMessage = new Receipt("message-id") {RefToMessageId = "ref-to-message-id"};
                signalMessage.IsDuplicated = false;

                InternalMessage internalMessage = new InternalMessageBuilder().WithSignalMessage(signalMessage).Build();

                // Act           
                // Execute the step twice.     
                StepResult stepResult = await Step.ExecuteAsync(internalMessage, CancellationToken.None);
                Assert.False(stepResult.InternalMessage.AS4Message.PrimarySignalMessage.IsDuplicated);

                stepResult = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.True(stepResult.InternalMessage.AS4Message.PrimarySignalMessage.IsDuplicated);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateUserMessageAsync()
            {
                // Arrange
                UserMessage userMessage = CreateUserMessage();
                await InsertDuplicateUserMessage(userMessage);
                InternalMessage internalMessage = new InternalMessageBuilder().WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;
                internalMessage.AS4Message.ReceivingPMode = pmode;

                // Act
                await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.NotApplicable);
            }
        }

        /// <summary>
        /// Create a <see cref="UserMessage"/> based on the configured Id's.
        /// </summary>
        /// <returns></returns>
        protected UserMessage CreateUserMessage()
        {
            return new UserMessage(_userMessageId) {RefToMessageId = _userMessageId};
        }
    }
}