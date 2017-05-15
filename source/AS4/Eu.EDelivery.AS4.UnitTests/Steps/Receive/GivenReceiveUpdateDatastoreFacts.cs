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

            Step = new SaveReceivedMessageStep(GetDataStoreContext, StubMessageBodyPersister.Default);
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
            public async Task ThenExecuteStepUpdatesDuplicateReceiptMessageAsync()
            {
                // Arrange
                SignalMessage signalMessage = new Receipt("message-id") { RefToMessageId = "ref-to-message-id" };
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

            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                AS4Message message = new AS4MessageBuilder().Build();
                var internalMessage = new InternalMessage(message);

                // Act
                StepResult result = await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesAsErrorAsync()
            {
                // Arrange
                SignalMessage errorMessage = CreateError();

                InternalMessage internalMessage =
                    new InternalMessageBuilder(errorMessage.RefToMessageId).WithSignalMessage(errorMessage).Build();
                internalMessage.AS4Message.SendingPMode = new SendingProcessingMode();
                internalMessage.AS4Message.ReceivingPMode = new ReceivingProcessingMode();

                // Act
                await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                await AssertOutMessages(errorMessage, OutStatus.Nack);
                await AssertInMessage(errorMessage);
            }

            [Fact]
            public async Task ThenExecuteStepUpdatesAsReceiptAsync()
            {
                // Arrange
                SignalMessage receiptMessage = CreateReceipt();
                InternalMessage internalMessage = CreateInternalMessageWith(receiptMessage);

                receiptMessage.RefToMessageId = internalMessage.AS4Message.PrimaryUserMessage.MessageId;

                // Act
                await Step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                await AssertOutMessages(receiptMessage, OutStatus.Ack);
                await AssertInMessage(receiptMessage);
            }

            private static InternalMessage CreateInternalMessageWith(SignalMessage receiptMessage)
            {
                InternalMessage internalMessage = new InternalMessageBuilder(receiptMessage.RefToMessageId)
                                .WithSignalMessage(receiptMessage).Build();

                internalMessage.AS4Message.SendingPMode = new SendingProcessingMode();
                internalMessage.AS4Message.ReceivingPMode = new ReceivingProcessingMode();

                return internalMessage;
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

    ////public class GivenReceiveUpdateDataStoreFacts
    ////{
    ////    [Fact]
    ////    public async Task ThenExecuteStepSucceedsWithDeliveringAsync()
    ////    {
    ////        // Arrange
    ////        UserMessage userMessage = CreateUserMessage();
    ////        InternalMessage internalMessage = new InternalMessageBuilder().WithUserMessage(userMessage).Build();

    ////        var pmode = new ReceivingProcessingMode();
    ////        pmode.Reliability.DuplicateElimination.IsEnabled = false;
    ////        pmode.Deliver.IsEnabled = true;
    ////        internalMessage.AS4Message.ReceivingPMode = pmode;

    ////        // Act
    ////        await Step.ExecuteAsync(internalMessage, CancellationToken.None);

    ////        // Assert
    ////        await AssertUserInMessageAsync(userMessage, m => m.Operation == Operation.ToBeDelivered);
    ////    }

    ////    /// <summary>
    ////    /// Create a <see cref="UserMessage" /> based on the configured Id's.
    ////    /// </summary>
    ////    /// <returns></returns>
    ////    protected UserMessage CreateUserMessage()
    ////    {
    ////        return new UserMessage(_userMessageId) { RefToMessageId = _userMessageId };
    ////    }
    ////}
}