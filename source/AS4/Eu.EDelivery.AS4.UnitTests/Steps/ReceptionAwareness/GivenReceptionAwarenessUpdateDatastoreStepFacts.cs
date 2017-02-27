using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.ReceptionAwareness;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using Assert = Xunit.Assert;

namespace Eu.EDelivery.AS4.UnitTests.Steps.ReceptionAwareness
{
    /// <summary>
    /// Testing <see cref="ReceptionAwarenessUpdateDatastoreStep"/>
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
                Entities.ReceptionAwareness awareness = base.CreateDefaultReceptionAwareness();
                ArrangeMessageIsAlreadyAwnsered(awareness.InternalMessageId);
                base.InsertReceptionAwareness(awareness);

                var internalMessage = new InternalMessage() { ReceptionAwareness = awareness };
                var step = new ReceptionAwarenessUpdateDatastoreStep();

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertReceptionAwareness(awareness.InternalMessageId, x => Assert.True(x.IsCompleted));
            }

            private void ArrangeMessageIsAlreadyAwnsered(string messageId)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    var inMessage = new Entities.InMessage { EbmsRefToMessageId = messageId };
                    context.InMessages.Add(inMessage);
                    context.SaveChanges();
                }
            }

            [Fact(Skip = "DataAwareness Agent is no longer responsible for increasing RetryCount")]
            public async Task ThenMessageMustResendAsync()
            {
                // Arrange
                Entities.ReceptionAwareness awareness = base.CreateDefaultReceptionAwareness();
                base.InsertReceptionAwareness(awareness);

                var internalMessage = new InternalMessage() { ReceptionAwareness = awareness };
                var step = new ReceptionAwarenessUpdateDatastoreStep();

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertReceptionAwareness(awareness.InternalMessageId, x =>
                {
                    Assert.NotEqual(awareness.LastSendTime, x.LastSendTime);
                    Assert.Equal(awareness.CurrentRetryCount + 1, x.CurrentRetryCount);
                });
            }

            [Fact]
            public async Task ThenMessageIsUnawnseredAsync()
            {
                // Arrange
                Entities.ReceptionAwareness awareness = base.CreateDefaultReceptionAwareness();
                awareness.CurrentRetryCount = awareness.TotalRetryCount;
                base.InsertReceptionAwareness(awareness);
                base.InsertOutMessage(awareness.InternalMessageId);

                var internalMessage = new InternalMessage() { ReceptionAwareness = awareness };
                var step = new ReceptionAwarenessUpdateDatastoreStep();

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertInMessage(awareness.InternalMessageId);
                AssertReceptionAwareness(awareness.InternalMessageId, x => Assert.True(x.IsCompleted));
            }

            private void AssertInMessage(string messageId)
            {
                using (var context = this.GetDataStoreContext())
                {
                    Entities.InMessage inMessage = context.InMessages
                        .FirstOrDefault(m => m.EbmsRefToMessageId.Equals(messageId));

                    Assert.NotNull(inMessage);
                }
            }

            private void AssertReceptionAwareness(string messageId, Action<Entities.ReceptionAwareness> condition)
            {
                using (var context = this.GetDataStoreContext())
                {
                    Entities.ReceptionAwareness awareness = context.ReceptionAwareness
                        .FirstOrDefault(a => a.InternalMessageId.Equals(messageId));

                    Assert.NotNull(awareness);
                    condition(awareness);
                }
            }
        }

        protected void InsertReceptionAwareness(Entities.ReceptionAwareness receptionAwareness)
        {
            using (var context = this.GetDataStoreContext())
            {
                context.ReceptionAwareness.Add(receptionAwareness);
                context.SaveChanges();
            }
        }

        protected void InsertOutMessage(string messageId)
        {
            using (var context = this.GetDataStoreContext())
            {
                var pmode = new SendingProcessingMode();
                string pmodeString = AS4XmlSerializer.Serialize(pmode);
                var outMessage = new Entities.OutMessage { EbmsMessageId = messageId, PMode = pmodeString };

                context.OutMessages.Add(outMessage);
                context.SaveChanges();
            }
        }

        protected Entities.ReceptionAwareness CreateDefaultReceptionAwareness()
        {
            return new Entities.ReceptionAwareness
            {
                CurrentRetryCount = 0,
                IsCompleted = false,
                InternalMessageId = "message-id",
                LastSendTime = DateTimeOffset.UtcNow.AddMinutes(-1),
                RetryInterval = "00:00:00",
                TotalRetryCount = 5
            };
        }
    }
}