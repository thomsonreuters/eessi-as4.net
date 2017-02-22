using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Common;
using Eu.EDelivery.AS4.UnitTests.Common;
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
            MapInitialization.InitializeMapper();

            this._mockedStep = new Mock<IStep>();
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
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertOutException(sharedId, e =>
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
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();
                internalMessage.AS4Message.SendingPMode.ExceptionHandling.NotifyMessageProducer = false;
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertOutException(sharedId, e =>
                {
                    Assert.NotNull(e);
                    Assert.Equal(Operation.NotApplicable, e.Operation);
                });
            }

            private void AssertOutException(string messageId, Action<OutException> assertAction)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    OutException outException = context.OutExceptions
                        .FirstOrDefault(e => e.EbmsRefToMessageId.Equals(messageId));
                    assertAction(outException);
                }
            }

            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithUpdatedOutMessageAsync(string sharedId)
            {
                // Arrange
                SetupMockedStep(sharedId);
                ResetStep();
                OutMessage outMessage = CreateDefaultOutMessage(sharedId);
                InsertOutMessage(outMessage);
                var internalMessage = new InternalMessage();
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
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
                using (var context = this.GetDataStoreContext())
                {
                    context.OutMessages.Add(outMessage);
                    context.SaveChanges();
                }
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> assertAction)
            {
                using (var context = this.GetDataStoreContext())
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

            this._mockedStep = new Mock<IStep>();
            this._mockedStep
                .Setup(s => s.ExecuteAsync(It.IsAny<InternalMessage>(), It.IsAny<CancellationToken>()))
                .Throws(as4Exception);
        }

        protected void ResetStep()
        {
            this._step = new OutExceptionStepDecorator(this._mockedStep.Object);
        }

        protected InternalMessage CreateDefaultInternalMessage()
        {
            return new InternalMessage
            {
                AS4Message =
                {
                    SendingPMode = new SendingProcessingMode
                    {
                        ExceptionHandling = {NotifyMessageProducer = true}
                    }
                }
            };
        }
    }
}
