using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Services;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
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
            IdGenerator.SetContext(StubConfig.Instance);
            this._userMessageId = Guid.NewGuid().ToString();
            var registry = new Registry();

            IInMessageService repository = new InMessageService(
                new DatastoreRepository(() => new DatastoreContext(base.Options)));

            base.Step = new ReceiveUpdateDatastoreStep(repository);
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
                SignalMessage signalMessage = CreateDefaultSignalMessage();
                signalMessage.IsDuplicated = false;

                Mock<IInMessageService> mockedMessageService = CreateMockedInMessageService();
                base.Step = new ReceiveUpdateDatastoreStep(mockedMessageService.Object);

                InternalMessage internalMessage = new InternalMessageBuilder()
                    .WithSignalMessage(signalMessage).Build();
                // Act
                StepResult stepResult = await base.Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.True(stepResult.InternalMessage.AS4Message.PrimarySignalMessage.IsDuplicated);
            }

            private Mock<IInMessageService> CreateMockedInMessageService()
            {
                var mockedMessageService = new Mock<IInMessageService>();
                mockedMessageService
                    .Setup(s => s.ContainsSignalMessageWithReferenceToMessageId(It.IsAny<string>()))
                    .Returns(true);

                return mockedMessageService;
            }

            private SignalMessage CreateDefaultSignalMessage()
            {
                return new SignalMessage(messageId: "message-id")
                {
                    RefToMessageId = "ref-to-message-id"
                };
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

            private void AddTestableDataToUserMessage(UserMessage userMessage)
            {
                userMessage.CollaborationInfo.Action = Constants.Namespaces.TestAction;
                userMessage.CollaborationInfo.Service.Name = Constants.Namespaces.TestService;
            }

            private async Task InsertDuplicateUserMessage(UserMessage userMessage)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    var inMessage = new InMessage {EbmsMessageId = userMessage.MessageId};
                    context.InMessages.Add(inMessage);
                    await context.SaveChangesAsync();
                }
            }

            private async Task AssertUserInMessageAsync(
                MessageUnit userMessage, Func<InMessage, bool> condition = null)
            {
                using (var context = new DatastoreContext(base.Options))
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