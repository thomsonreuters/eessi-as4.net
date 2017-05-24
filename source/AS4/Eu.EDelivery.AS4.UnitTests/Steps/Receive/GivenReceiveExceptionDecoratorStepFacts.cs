using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing <see cref="ReceiveExceptionStepDecorator" />
    /// </summary>
    public class GivenReceiveExceptionDecoratorStepFacts : GivenDatastoreFacts
    {
        public GivenReceiveExceptionDecoratorStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenReceiveExceptionDecoratorStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithoutThrowedExceptionAsync()
            {
                // Arrange
                IStep sut = GetCatchedCompositeSteps();
                var internalMessage = new InternalMessage();

                // Act
                StepResult result = await sut.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                Assert.NotNull(result.InternalMessage.AS4Message);
                Assert.Equal(internalMessage, result.InternalMessage);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithAS4ExceptionThrowedAsync()
            {
                // Arrange
                var stubStep = new SaboteurStep(CreateAS4Exception());
                IStep sut = GetCatchedCompositeSteps(stubStep);

                // Act
                StepResult result = await sut.ExecuteAsync(DummyMessage(), CancellationToken.None);

                // Assert
                Assert.NotNull(result.InternalMessage.AS4Message);
                Assert.NotNull(result.InternalMessage.AS4Message.ReceivingPMode);
            }

            [Theory]
            [InlineData("shared-message-id")]
            public async Task ThenExecuteStepSucceedsWithInsertedInExceptionAsync(string messageId)
            {
                // Arrange
                var stubStep = new SaboteurStep(CreateAS4Exception(messageId));
                IStep sut = GetCatchedCompositeSteps(stubStep);

                // Act
                await sut.ExecuteAsync(DummyMessage(), CancellationToken.None);

                // Assert
                AssertInException(messageId, Assert.NotNull);
            }

            [Fact(Skip="I think this a component-test is better suited here")]
            public async Task ThenExecuteStepSucceedsWithCallbackReplyPatternAsync()
            {
                // Why a component test: we're dependent on a sending pmode, a receiving pmode 
                // and they must exist, otherwise other exceptions will be thrown.

                // Arrange
                InternalMessage internalMessage = DummyMessage();
                internalMessage.AS4Message.ReceivingPMode.ErrorHandling.ReplyPattern = ReplyPattern.Callback;
                internalMessage.AS4Message.ReceivingPMode.ErrorHandling.SendingPMode = "";

                var stubStep = new SaboteurStep(CreateAS4Exception());
                IStep sut = GetCatchedCompositeSteps(stubStep);

                // Act
                StepResult result = await sut.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert                
                Assert.NotNull(result.InternalMessage.AS4Message);
                // TODO: could we assert more explicitly on somethting like AS4Message.Empty oid.
                Assert.Empty(result.InternalMessage.AS4Message.UserMessages);
                Assert.Empty(result.InternalMessage.AS4Message.SignalMessages);
            }

            private void AssertInException(string messageId, Action<InException> condition)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InException inException =
                        context.InExceptions.FirstOrDefault(e => e.EbmsRefToMessageId.Equals(messageId));
                    condition(inException);
                }
            }
        }

        private IStep GetCatchedCompositeSteps(IStep catchedStep = null)
        {
            return new CompositeStep(
                new ReceiveExceptionStepDecorator(catchedStep ?? new SinkStep()),
                new CreateAS4ErrorStep(new StubMessageBodyStore(), () => new DatastoreContext(Options)),
                new SendAS4ErrorStep());
        }

        protected ReceivingProcessingMode GetStubReceivingPMode()
        {
            return new ReceivingProcessingMode {ReceiptHandling = {UseNNRFormat = false, SendingPMode = "pmode"}};
        }

        protected InternalMessage DummyMessage()
        {
            var as4Message = new AS4Message
            {
                ReceivingPMode = GetStubReceivingPMode(),
                UserMessages = new[] {new UserMessage("message-id")}
            };
            return new InternalMessage(as4Message);
        }

        private AS4Exception CreateAS4Exception(string messageId = "ignored-string")
        {
            return AS4ExceptionBuilder
                .WithDescription("Testing AS4 Exception")
                .WithPModeString(AS4XmlSerializer.ToString(GetStubReceivingPMode()))
                .WithMessageIds(messageId)
                .Build();
        }
    }
}