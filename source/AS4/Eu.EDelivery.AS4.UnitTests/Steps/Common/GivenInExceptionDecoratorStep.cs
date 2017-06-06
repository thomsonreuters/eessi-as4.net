using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Common;
using Eu.EDelivery.AS4.UnitTests.Common;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Common
{
    /// <summary>
    /// Testing <see cref="InExceptionStepDecorator"/>
    /// </summary>
    public class GivenInExceptionDecoratorStepFacts : GivenDatastoreFacts
    {
        private InExceptionStepDecorator _step;
        private Mock<IStep> _mockedStep;

        public GivenInExceptionDecoratorStepFacts()
        {
            _mockedStep = new Mock<IStep>();

            ResetStep();
        }

        public class GivenValidArguments : GivenInExceptionDecoratorStepFacts
        {
            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithInsertedInExceptionAsync(string sharedId)
            {
                // Arrange
                AS4Exception as4Exception = CreateDefaultAS4Exception(sharedId);
                SetupMockedStep(as4Exception);
                ResetStep();

                var internalMessage = new MessagingContext(new AS4Message());

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertInException(sharedId, exception =>
                {
                    Assert.NotNull(exception);
                    Assert.Equal(Operation.NotApplicable, exception.Operation);
                });
            }

            private void AssertInException(string messageId, Action<InException> assertAction)
            {
                using (var context = GetDataStoreContext())
                {
                    InException inException = context.InExceptions
                        .FirstOrDefault(e => e.EbmsRefToMessageId.Equals(messageId));
                    assertAction(inException);
                }
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithToBeNotifiedInExceptionAsync(string sharedId)
            {
                // Arrange
                AS4Exception as4Exception = CreateDefaultAS4Exception(sharedId);
                var receivePMode = new ReceivingProcessingMode { ExceptionHandling = { NotifyMessageConsumer = true } };
                as4Exception.PMode = AS4XmlSerializer.ToString(receivePMode);
                SetupMockedStep(as4Exception);
                ResetStep();

                var internalMessage = new MessagingContext(new AS4Message());

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);


                // Assert
                AssertInException(sharedId, exception =>
                {
                    Assert.NotNull(exception);
                    Assert.Equal(Operation.ToBeNotified, exception.Operation);
                });
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithUpdatedInMessageAsync(string sharedId)
            {
                // Arrange
                AS4Exception as4Exception = CreateDefaultAS4Exception(sharedId);
                SetupMockedStep(as4Exception);
                ResetStep();

                using (var context = GetDataStoreContext())
                {
                    InMessage inMessage = CreateDefaultInMessage(sharedId);
                    context.InMessages.Add(inMessage);

                    await context.SaveChangesAsync();
                }
                var internalMessage = new MessagingContext(new AS4Message());

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertInMessage(sharedId, message =>
                {
                    Assert.NotNull(message);
                    Assert.Equal(InStatus.Exception, message.Status);
                });
            }

            private static InMessage CreateDefaultInMessage(string messageId)
            {
                return new InMessage
                {
                    EbmsMessageId = messageId,
                    Status = InStatus.Notified
                };
            }

            private void AssertInMessage(string messageId, Action<InMessage> assertAction)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InMessage inMessage = context.InMessages
                        .FirstOrDefault(e => e.EbmsMessageId.Equals(messageId));
                    assertAction(inMessage);
                }
            }
        }

        protected void SetupMockedStep(AS4Exception as4Exception)
        {
            _mockedStep = new Mock<IStep>();
            _mockedStep
                .Setup(s => s.ExecuteAsync(It.IsAny<MessagingContext>(), It.IsAny<CancellationToken>()))
                .Throws(as4Exception);
        }

        private static AS4Exception CreateDefaultAS4Exception(string messageId)
        {
            return AS4ExceptionBuilder
                .WithDescription("Test AS4 Exception")
                .WithMessageIds(messageId)
                .Build();
        }

        protected void ResetStep()
        {
            _step = new InExceptionStepDecorator(_mockedStep.Object);
        }
    }
}