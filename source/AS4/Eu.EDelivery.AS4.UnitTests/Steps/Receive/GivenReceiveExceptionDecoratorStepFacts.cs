using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.Steps.Services;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Utilities;
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
            IdGenerator.SetContext(StubConfig.Instance);

            SetupMockedCatchedStep();

            var datastoreRepository = new DatastoreRepository(() => new DatastoreContext(base.Options));
            this._step = new ReceiveExceptionStepDecorator(
                this._mockedCatchedStep.Object,
                new OutMessageService(datastoreRepository),
                new InExceptionService(datastoreRepository));
        }

        public class GivenValidArguments : GivenReceiveExceptionDecoratorStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithoutThrowedExceptionAsync()
            {
                // Arrange
                var internalMessage = new InternalMessage();
                // Act
                StepResult result = await base._step
                    .ExecuteAsync(internalMessage, CancellationToken.None);
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
                var datastoreRepository = new DatastoreRepository(() => new DatastoreContext(base.Options));
                this._step = new ReceiveExceptionStepDecorator(
                    this._mockedCatchedStep.Object,
                    new OutMessageService(datastoreRepository),
                    new InExceptionService(datastoreRepository));
            }

            private void AssertInException(string messageId, Action<InException> condition)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    InException inException = context.InExceptions
                        .FirstOrDefault(e => e.EbmsRefToMessageId.Equals(messageId));
                    condition(inException);
                }
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> condition)
            {
                using (var context = new DatastoreContext(base.Options))
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
                var datastoreRepository = new DatastoreRepository(() => new DatastoreContext(StubConfig.Instance));
                this._step = new ReceiveExceptionStepDecorator(
                    this._mockedCatchedStep.Object,
                    new OutMessageService(datastoreRepository),
                    new InExceptionService(datastoreRepository));
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
                ReceiptHandling = {UseNNRFormat = false, SendingPMode = "pmode"}
            };
        }

        protected InternalMessage CreateDefaultInternalMessage()
        {
            var as4Message = new AS4Message
            {
                ReceivingPMode = GetReceivingPMode(),
                UserMessages = new[] {GetUserMessage()}
            };
            return new InternalMessage(as4Message);
        }

        protected UserMessage GetUserMessage()
        {
            return new UserMessage("message-id");
        }

        protected void SetupCatchedStepWithException(string messageId = "dummy-message-id")
        {
            AS4Exception as4Exception = new AS4ExceptionBuilder()
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