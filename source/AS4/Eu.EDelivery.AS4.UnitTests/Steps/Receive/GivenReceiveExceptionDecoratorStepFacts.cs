using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="ReceiveExceptionStepDecorator"/>
    /// </summary>
    public class GivenReceiveExceptionDecoratorStepFacts : GivenDatastoreFacts
    {
        private Mock<IStep> _mockedCatchedStep;
        private ReceiveExceptionStepDecorator _step;

        public GivenReceiveExceptionDecoratorStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);

            MapInitialization.InitializeMapper();

            SetupMockedCatchedStep();

            this._step = new ReceiveExceptionStepDecorator(this._mockedCatchedStep.Object);
        }

        public class GivenValidArguments : GivenReceiveExceptionDecoratorStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithoutThrowedExceptionAsync()
            {
                // Arrange
                var internalMessage = new InternalMessage();
                // Act

                StepResult result = await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Assert.NotNull(result.InternalMessage.AS4Message);
                Assert.Equal(internalMessage, result.InternalMessage);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithAS4ExceptionThrowedAsync()
            {
                // Arrange
                SetupCatchedStepWithException();
                ResetTestedStep();
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();

                // Act

                StepResult result = await base._step
                    .ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(result.InternalMessage.AS4Message);
                Assert.NotNull(result.InternalMessage.AS4Message.ReceivingPMode);

            }

            [Fact(Skip = "Wait till the Step Decorator is refactored towards a better design")]
            public async Task ThenExecuteStepSucceedsWithSignedErrorMessageAsync()
            {
                // Arrange
                SetupCatchedStepWithException();
                ResetTestedStep();
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();

                // Act
                StepResult result = await base._step
                    .ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(result.InternalMessage.AS4Message);
                Assert.True(result.InternalMessage.AS4Message.IsSigned);

            }

            [Theory, InlineData("shared-message-id")]
            public async Task ThenExecuteStepSucceedsWithInsertedInExceptionAsync(string messageId)
            {
                // Arrange
                SetupCatchedStepWithException(messageId);
                ResetTestedStep();
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertInException(messageId, Assert.NotNull);

            }

            [Theory, InlineData("shared-message-id")]
            public async Task ThenExecuteStepSucceedsWithInsertedOutMessageAsync(string messageId)
            {
                // Arrange
                SetupCatchedStepWithException(messageId);
                ResetTestedStep();
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertOutMessage(messageId, Assert.NotNull);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithCallbackReplyPatternAsync()
            {
                // Arrange
                SetupCatchedStepWithException();
                ResetTestedStep();
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();
                internalMessage.AS4Message.ReceivingPMode.ErrorHandling.ReplyPattern = ReplyPattern.Callback;
                // Act
                StepResult result = await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                Assert.NotNull(result.InternalMessage.AS4Message);
                Assert.Empty(result.InternalMessage.AS4Message.UserMessages);
                Assert.Empty(result.InternalMessage.AS4Message.SignalMessages);
            }

            private void ResetTestedStep()
            {
                this._step = new ReceiveExceptionStepDecorator(
                    this._mockedCatchedStep.Object);
            }

            private void AssertInException(string messageId, Action<InException> condition)
            {
                using (var context = GetDataStoreContext())
                {
                    InException inException = context.InExceptions
                        .FirstOrDefault(e => e.EbmsRefToMessageId.Equals(messageId));
                    condition(inException);
                }
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> condition)
            {
                using (var context = GetDataStoreContext())
                {
                    OutMessage outMessage = context.OutMessages
                        .FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));
                    condition(outMessage);
                }
            }
        }

        public class GivenInvalidArguments : GivenReceiveExceptionDecoratorStepFacts
        {
            [Fact(Skip = "Wait till the Step Decorator is refactored towards a better design")]
            public async Task ThenCertificateCannotBeRetrievedFromStoreAsync()
            {
                // Arrange
                SetupCatchedStepWithException();
                ResetTestedStepWithInvalidCertificateRepository();
                InternalMessage internalMessage = CreateDefaultInternalMessage();

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(()
                    => base._step.ExecuteAsync(internalMessage, CancellationToken.None));

            }

            private void ResetTestedStepWithInvalidCertificateRepository()
            {
                this._step = new ReceiveExceptionStepDecorator(
                    this._mockedCatchedStep.Object);
            }
        }

        private void SetupMockedCatchedStep()
        {
            this._mockedCatchedStep = new Mock<IStep>();

            this._mockedCatchedStep
              .Setup(s => s.ExecuteAsync(It.IsAny<InternalMessage>(), It.IsAny<CancellationToken>()))
              .Returns((InternalMessage m, CancellationToken c) => StepResult.SuccessAsync(m));

        }

        protected ReceivingProcessingMode GetReceivingPMode()
        {
            return new ReceivingProcessingMode
            {
                ReceiptHandling = { UseNNRFormat = false, SendingPMode = "pmode" }
            };
        }

        protected InternalMessage CreateDefaultInternalMessage()
        {
            var as4Message = new AS4Message
            {
                ReceivingPMode = GetReceivingPMode(),
                UserMessages = new[] { GetUserMessage() }
            };
            return new InternalMessage(as4Message);
        }

        protected UserMessage GetUserMessage()
        {
            return new UserMessage("message-id");
        }

        protected void SetupCatchedStepWithException(string messageId = "dummy-message-id")
        {
            AS4Exception as4Exception = AS4ExceptionBuilder
                .WithDescription("Testing AS4 Exception")
                .WithPModeString(AS4XmlSerializer.Serialize(GetReceivingPMode()))
                .WithMessageIds(messageId)
                .Build();


            this._mockedCatchedStep
                .Setup(s => s.ExecuteAsync(It.IsAny<InternalMessage>(), It.IsAny<CancellationToken>()))
                .Throws(as4Exception);

        }
    }
}