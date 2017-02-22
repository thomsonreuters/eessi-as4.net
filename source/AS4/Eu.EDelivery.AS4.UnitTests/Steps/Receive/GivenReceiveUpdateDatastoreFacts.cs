using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Eu.EDelivery.AS4.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="ReceiveUpdateDatastoreStep"/>
    /// </summary>
    public class GivenReceiveUpdateDatastoreFacts : GivenDatastoreStepFacts
    {
        private readonly string _userMessageId;

        public GivenReceiveUpdateDatastoreFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
            this._userMessageId = Guid.NewGuid().ToString();

            base.Step = new ReceiveUpdateDatastoreStep();
        }

        protected UserMessage GetUserMessage()
        {
            return new UserMessage(this._userMessageId)
            {
                RefToMessageId = this._userMessageId
            };
        }

        /// <summary>
        /// Testing the Step with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenReceiveUpdateDatastoreFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithDeliveringAsync()
            {
                // Arrange
                UserMessage userMessage = base.GetUserMessage();
                InternalMessage internalMessage = new InternalMessageBuilder()
                    .WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = false;
                pmode.Deliver.IsEnabled = true;
                internalMessage.AS4Message.ReceivingPMode = pmode;

                // Act
                await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.ToBeDelivered);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateReceiptMessageAsync()
            {
                // Arrange
                SignalMessage signalMessage = new Receipt("message-id") { RefToMessageId = "ref-to-message-id" };
                signalMessage.IsDuplicated = false;

                base.Step = new ReceiveUpdateDatastoreStep();

                InternalMessage internalMessage = new InternalMessageBuilder()
                    .WithSignalMessage(signalMessage).Build();
                
                // Act           
                // Execute the step twice.     
                var stepResult = await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);
                Assert.False(stepResult.InternalMessage.AS4Message.PrimarySignalMessage.IsDuplicated);

                stepResult = await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Assert.True(stepResult.InternalMessage.AS4Message.PrimarySignalMessage.IsDuplicated);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesDuplicateUserMessageAsync()
            {
                // Arrange
                UserMessage userMessage = base.GetUserMessage();
                await InsertDuplicateUserMessage(userMessage);
                InternalMessage internalMessage = new InternalMessageBuilder()
                    .WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;
                internalMessage.AS4Message.ReceivingPMode = pmode;

                // Act
                await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.NotApplicable);
            }

            [Fact]
            public async Task ThenExecuteStepIsTestUserMessageAsync()
            {
                // Arrange
                UserMessage userMessage = base.GetUserMessage();
                AddTestableDataToUserMessage(userMessage);
                InternalMessage internalMessage = new InternalMessageBuilder()
                    .WithUserMessage(userMessage).Build();

                var pmode = new ReceivingProcessingMode();
                pmode.Reliability.DuplicateElimination.IsEnabled = true;
                internalMessage.AS4Message.ReceivingPMode = pmode;

                // Act
                await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);


                // Assert
                await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.NotApplicable);
            }

            private static void AddTestableDataToUserMessage(UserMessage userMessage)
            {
                userMessage.CollaborationInfo.Action = Constants.Namespaces.TestAction;
                userMessage.CollaborationInfo.Service.Value = Constants.Namespaces.TestService;
            }

            private async Task InsertDuplicateUserMessage(UserMessage userMessage)
            {
                using (var context = this.GetDataStoreContext())
                {
                    var inMessage = new InMessage { EbmsMessageId = userMessage.MessageId };
                    context.InMessages.Add(inMessage);
                    await context.SaveChangesAsync();
                }
            }

            private async Task AssertUserInMessageAsync(
                MessageUnit userMessage, Func<InMessage, bool> condition = null)
            {
                using (var context = this.GetDataStoreContext())
                {
                    InMessage inMessage = await context.InMessages.FirstOrDefaultAsync(
                        m => m.EbmsMessageId.Equals(userMessage.MessageId));

                    Assert.NotNull(inMessage);
                    Assert.Equal(Entities.MessageType.UserMessage, inMessage.EbmsMessageType);
                    if (condition != null) Assert.True(condition(inMessage));
                }
            }
        }
    }
}