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

                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);

                // Act
                StepResult result = await sut.ExecuteAsync(context, CancellationToken.None);

                // Assert
                Assert.NotNull(result.MessagingContext.AS4Message);
                Assert.Equal(context, result.MessagingContext);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithAS4ExceptionThrowedAsync()
            {
                // Arrange
                var stubStep = new SaboteurStep(await CreateAS4Exception());
                IStep sut = GetCatchedCompositeSteps(stubStep);

                // Act
                StepResult result = await sut.ExecuteAsync(DummyMessage(), CancellationToken.None);

                // Assert
                Assert.NotNull(result.MessagingContext.AS4Message);
                Assert.NotNull(result.MessagingContext.ReceivingPMode);
            }

            [Theory]
            [InlineData("shared-message-id")]
            public async Task ThenExecuteStepSucceedsWithInsertedInExceptionAsync(string messageId)
            {
                // Arrange
                var stubStep = new SaboteurStep(await CreateAS4Exception(messageId));
                IStep sut = GetCatchedCompositeSteps(stubStep);

                // Act
                await sut.ExecuteAsync(DummyMessage(), CancellationToken.None);

                // Assert
                AssertInException(messageId, Assert.NotNull);
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
                new SendAS4SignalMessageStep());
        }

        protected ReceivingProcessingMode GetStubReceivingPMode()
        {
            return new ReceivingProcessingMode { ReceiptHandling = { UseNNRFormat = false, SendingPMode = "pmode" } };
        }

        protected MessagingContext DummyMessage()
        {

            return new MessagingContext(AS4Message.Create(new UserMessage("message-id")), MessagingContextMode.Unknown)
            {
                ReceivingPMode = GetStubReceivingPMode(),
                SendingPMode = new SendingProcessingMode()
            };
        }

        private async Task<AS4Exception> CreateAS4Exception(string messageId = "ignored-string")
        {
            return AS4ExceptionBuilder
                .WithDescription("Testing AS4 Exception")
                .WithPModeString(await AS4XmlSerializer.ToStringAsync(GetStubReceivingPMode()))
                .WithMessageIds(messageId)
                .Build();
        }
    }
}