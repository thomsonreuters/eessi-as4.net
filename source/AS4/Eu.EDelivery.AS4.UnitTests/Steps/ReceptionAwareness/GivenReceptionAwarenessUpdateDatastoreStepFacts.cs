using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
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
        public GivenReceptionAwarenessUpdateDatastoreStepFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenReceptionAwarenessUpdateDatastoreStepFacts
        {
            [Fact]
            public async Task ThenMessageIsAlreadyAwnseredAsync()
            {
                // Arrange
                EntityReceptionAwareness awareness = InsertAlreadyAnsweredMessage();

                var internalMessage = new MessagingContext(awareness);
                var step = new ReceptionAwarenessUpdateDatastoreStep(StubMessageBodyStore.Default, GetDataStoreContext);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertReceptionAwareness(
                    awareness.InternalMessageId,
                    x => Assert.Equal(ReceptionStatus.Completed, x.Status));
            }

            private EntityReceptionAwareness InsertAlreadyAnsweredMessage()
            {
                EntityReceptionAwareness awareness = CreateDefaultReceptionAwareness();

                InsertReceptionAwareness(awareness);
                ArrangeMessageIsAlreadyAnswered(awareness.InternalMessageId);

                return awareness;
            }

            private void ArrangeMessageIsAlreadyAnswered(string messageId)
            {
                using (var context = new DatastoreContext(Options))
                {
                    var outMessage = new OutMessage {EbmsMessageId = messageId, Status = OutStatus.Ack};
                    context.OutMessages.Add(outMessage);

                    context.SaveChanges();
                }
            }

            [Fact]
            public async Task ThenMessageIsUnansweredAsync()
            {
                // Arrange
                EntityReceptionAwareness awareness = CreateDefaultReceptionAwareness();
                awareness.CurrentRetryCount = awareness.TotalRetryCount;

                InsertReceptionAwareness(awareness);
                await InsertOutMessage(awareness.InternalMessageId);

                var internalMessage = new MessagingContext(awareness);
                var step = new ReceptionAwarenessUpdateDatastoreStep(StubMessageBodyStore.Default, GetDataStoreContext);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertNotNullInMessage(awareness.InternalMessageId);
                AssertReceptionAwareness(
                    awareness.InternalMessageId,
                    x => Assert.Equal(ReceptionStatus.Completed, x.Status));
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
                EntityReceptionAwareness awareness = CreateDefaultReceptionAwareness();
                awareness.CurrentRetryCount = 0;

                // When the DataReceiver receives a ReceptionAwareness item, it's status is Locked to Busy.
                awareness.Status = ReceptionStatus.Busy;

                InsertReceptionAwareness(awareness);
                await InsertOutMessage(awareness.InternalMessageId);

                var internalMessage = new MessagingContext(awareness);
                var step = new ReceptionAwarenessUpdateDatastoreStep(StubMessageBodyStore.Default, GetDataStoreContext);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertOutMessage(awareness.InternalMessageId, x => Assert.Equal(Operation.ToBeSent, x.Operation));
                AssertReceptionAwareness(
                    awareness.InternalMessageId,
                    x => Assert.Equal(ReceptionStatus.Pending, x.Status));
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
                        context.ReceptionAwareness.FirstOrDefault(a => a.InternalMessageId.Equals(messageId));

                    Assert.NotNull(awareness);
                    condition(awareness);
                }
            }
        }

        protected void InsertReceptionAwareness(EntityReceptionAwareness receptionAwareness)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                context.ReceptionAwareness.Add(receptionAwareness);
                context.SaveChanges();
            }
        }

        protected async Task InsertOutMessage(string messageId)
        {
            var pmode = new SendingProcessingMode();
            string pmodeString = await AS4XmlSerializer.ToStringAsync(pmode);
            var outMessage = new OutMessage { EbmsMessageId = messageId, PMode = pmodeString };

            GetDataStoreContext.InsertOutMessage(outMessage);
        }

        protected EntityReceptionAwareness CreateDefaultReceptionAwareness()
        {
            return new EntityReceptionAwareness
            {
                CurrentRetryCount = 0,
                Status = ReceptionStatus.Pending,
                InternalMessageId = "message-id",
                LastSendTime = DateTimeOffset.Now.AddMinutes(-1),
                RetryInterval = "00:00:00",
                TotalRetryCount = 5
            };
        }
    }
}