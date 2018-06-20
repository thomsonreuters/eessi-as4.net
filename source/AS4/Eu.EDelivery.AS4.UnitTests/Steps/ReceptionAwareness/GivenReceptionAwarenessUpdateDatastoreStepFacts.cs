using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.ReceptionAwareness;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;
using EntityReceptionAwareness = Eu.EDelivery.AS4.Entities.ReceptionAwareness;

namespace Eu.EDelivery.AS4.UnitTests.Steps.ReceptionAwareness
{
    /// <summary>
    /// Testing <see cref="ReceptionAwarenessUpdateDatastoreStep" />
    /// </summary>
    public class GivenReceptionAwarenessUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        public class GivenValidArguments : GivenReceptionAwarenessUpdateDatastoreStepFacts
        {
            [Fact]
            public async Task ThenMessageIsAlreadyAnsweredAsync()
            {
                // Arrange
                EntityReceptionAwareness awareness = InsertAlreadyAnsweredMessage();

                var internalMessage = new MessagingContext(awareness);
                IStep step = CreateUpdateReceptionAwarenessStep();

                // Act
                await step.ExecuteAsync(internalMessage);

                // Assert
                AssertReceptionAwareness(
                    awareness.RefToEbmsMessageId,
                    x => Assert.Equal(ReceptionStatus.Completed, x.Status.ToEnum<ReceptionStatus>()));
            }

            private EntityReceptionAwareness InsertAlreadyAnsweredMessage()
            {
                const string ebmsMessageId = "message-id";

                using (var context = GetDataStoreContext())
                {
                    var outMessage = new OutMessage(ebmsMessageId: ebmsMessageId);
                    outMessage.SetStatus(OutStatus.Ack);
                    context.OutMessages.Add(outMessage);

                    context.SaveChanges();

                    EntityReceptionAwareness awareness = CreateDefaultReceptionAwarenessFor(outMessage);

                    context.ReceptionAwareness.Add(awareness);

                    context.SaveChanges();

                    return awareness;
                }
            }

            [Fact]
            public async Task ThenMessageIsUnansweredAsync()
            {
                // Arrange
                EntityReceptionAwareness awareness = CreateOutMessageWithReceptionAwareness();
                awareness.CurrentRetryCount = awareness.TotalRetryCount;

                var messagingContext = new MessagingContext(awareness);
                IStep step = CreateUpdateReceptionAwarenessStep();

                // Act
                await step.ExecuteAsync(messagingContext);

                // Assert
                AssertNotNullInMessage(awareness.RefToEbmsMessageId);
                AssertReceptionAwareness(
                    awareness.RefToEbmsMessageId,
                    x => Assert.Equal(ReceptionStatus.Completed, x.Status.ToEnum<ReceptionStatus>()));
            }

            private void AssertNotNullInMessage(string messageId)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    InMessage inMessage = context.InMessages.FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));

                    Assert.NotNull(inMessage);
                }
            }

            [Fact]
            public async Task ThenStatusIsResetToPending()
            {
                // Arrange
                EntityReceptionAwareness awareness =
                    CreateOutMessageWithReceptionAwareness(currentRetryCount: 0, status: ReceptionStatus.Busy);

                var messagingContext = new MessagingContext(awareness);
                IStep step = CreateUpdateReceptionAwarenessStep();

                // Act
                await step.ExecuteAsync(messagingContext);

                // Assert
                AssertOutMessage(awareness.RefToEbmsMessageId, x => Assert.Equal(Operation.ToBeSent, x.Operation.ToEnum<Operation>()));
                AssertReceptionAwareness(
                    awareness.RefToEbmsMessageId,
                    x => Assert.Equal(ReceptionStatus.Pending, x.Status.ToEnum<ReceptionStatus>()));
            }

            private EntityReceptionAwareness CreateOutMessageWithReceptionAwareness(int currentRetryCount = 0, ReceptionStatus status = ReceptionStatus.Pending)
            {
                string ebmsMessageId = Guid.NewGuid().ToString();

                using (var context = GetDataStoreContext())
                {
                    var outMessage = new OutMessage(ebmsMessageId: ebmsMessageId);
                    outMessage.SetPModeInformation(new SendingProcessingMode());
                    context.OutMessages.Add(outMessage);

                    context.SaveChanges();

                    EntityReceptionAwareness awareness = CreateDefaultReceptionAwarenessFor(outMessage);

                    awareness.CurrentRetryCount = currentRetryCount;
                    awareness.SetStatus(status);

                    context.ReceptionAwareness.Add(awareness);

                    context.SaveChanges();

                    return awareness;
                }
            }

            private IStep CreateUpdateReceptionAwarenessStep()
            {
                return new ReceptionAwarenessUpdateDatastoreStep(StubConfig.Default, _messageBodyStore, GetDataStoreContext);
            }

            private void AssertOutMessage(string messagId, Action<OutMessage> condition)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    OutMessage outMessage = context.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messagId));

                    Assert.NotNull(outMessage);
                    condition(outMessage);
                }
            }

            private void AssertReceptionAwareness(string messageId, Action<EntityReceptionAwareness> condition)
            {
                using (DatastoreContext context = GetDataStoreContext())
                {
                    EntityReceptionAwareness awareness =
                        context.ReceptionAwareness.FirstOrDefault(a => a.RefToEbmsMessageId.Equals(messageId));

                    Assert.NotNull(awareness);
                    condition(awareness);
                }
            }
        }

        protected EntityReceptionAwareness CreateDefaultReceptionAwarenessFor(OutMessage outMessage)
        {
            var receptionAwareness = new EntityReceptionAwareness(outMessage.Id, outMessage.EbmsMessageId)
            {
                CurrentRetryCount = 0,
                LastSendTime = DateTimeOffset.Now.AddMinutes(-1),
                RetryInterval = "00:00:00",
                TotalRetryCount = 5
            };

            receptionAwareness.SetStatus(ReceptionStatus.Pending);

            return receptionAwareness;
        }
    }
}