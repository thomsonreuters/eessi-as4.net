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
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Common;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Common
{
    /// <summary>
    /// Testing <see cref="OutExceptionStepDecorator"/>
    /// </summary>
    public class GivenOutExceptionDecoratorStepFacts : GivenDatastoreFacts
    {
        private OutExceptionStepDecorator _step;
        private Mock<IStep> _mockedStep;

        public GivenOutExceptionDecoratorStepFacts()
        {
            _mockedStep = new Mock<IStep>();
            ResetStep();
        }

        public class GivenValidArguments : GivenOutExceptionDecoratorStepFacts
        {
            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithInsertedOutExceptionAsync(string sharedId)
            {
                // Arrange
                SetupMockedStep(sharedId);
                ResetStep();
                MessagingContext messagingContext = base.CreateDefaultInternalMessage();

                // Act
                await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                GetDataStoreContext.AssertOutException(sharedId, e =>
                {
                    Assert.NotNull(e);
                    Assert.Equal(Operation.ToBeNotified, e.Operation);
                });
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithoutExceptionNotifyProducer(string sharedId)
            {
                // Arrange
                SetupMockedStep(sharedId);
                ResetStep();
                MessagingContext messagingContext = base.CreateDefaultInternalMessage();
                messagingContext.SendingPMode.ExceptionHandling.NotifyMessageProducer = false;

                // Act
                await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                GetDataStoreContext.AssertOutException(sharedId, e =>
                {
                    Assert.NotNull(e);
                    Assert.Equal(Operation.NotApplicable, e.Operation);
                });
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithUpdatedOutMessageAsync(string sharedId)
            {
                // Arrange
                SetupMockedStep(sharedId);
                ResetStep();
                OutMessage outMessage = CreateDefaultOutMessage(sharedId);
                InsertOutMessage(outMessage);

                var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);

                // Act
                await _step.ExecuteAsync(context, CancellationToken.None);

                // Assert
                AssertOutMessage(sharedId, m =>
                {
                    Assert.NotNull(m);
                    Assert.Equal(MessageType.Error, m.EbmsMessageType);
                });
            }

            private static OutMessage CreateDefaultOutMessage(string messageId)
            {
                return new OutMessage
                {
                    EbmsMessageId = messageId,
                    EbmsMessageType = MessageType.UserMessage
                };
            }

            private void InsertOutMessage(OutMessage outMessage)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    context.OutMessages.Add(outMessage);
                    context.SaveChanges();
                }
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> assertAction)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    OutMessage outMessage = context.OutMessages
                        .FirstOrDefault(e => e.EbmsMessageId.Equals(messageId));
                    assertAction(outMessage);
                }
            }
        }

        protected void SetupMockedStep(string messageId)
        {
            AS4Exception as4Exception = AS4ExceptionBuilder
                .WithDescription("Test AS4 Exception")
                .WithMessageIds(messageId)
                .Build();

            _mockedStep = new Mock<IStep>();
            _mockedStep
                .Setup(s => s.ExecuteAsync(It.IsAny<MessagingContext>(), It.IsAny<CancellationToken>()))
                .Throws(as4Exception);
        }

        protected void ResetStep()
        {
            _step = new OutExceptionStepDecorator(_mockedStep.Object);
        }

        protected MessagingContext CreateDefaultInternalMessage()
        {
            return new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown)
            {
                SendingPMode = new SendingProcessingMode { ExceptionHandling = { NotifyMessageProducer = true } }
            };
        }
    }
}
