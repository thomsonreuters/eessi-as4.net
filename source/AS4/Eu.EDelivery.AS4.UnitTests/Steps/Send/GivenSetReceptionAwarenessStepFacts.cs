using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using EntityReceptionAwareness = Eu.EDelivery.AS4.Entities.ReceptionAwareness;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SetReceptionAwarenessStep" />
    /// </summary>
    public class GivenSetReceptionAwarenessStepFacts : GivenDatastoreFacts
    {
        protected readonly IDatastoreRepository Repository;

        public GivenSetReceptionAwarenessStepFacts()
        {
            Repository = new DatastoreRepository(InMemoryDatastore());
        }

        public class GivenValidArguments : GivenSetReceptionAwarenessStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepInsertsReceptionAwarenessAsync()
            {
                // Arrange
                const string messageId = "message-id";
                AS4.Model.PMode.ReceptionAwareness receptionAwareness = CreatePModeReceptionAwareness();
                InternalMessage internalMessage = CreateDefaultInternalMessage(messageId, receptionAwareness);
                var step = new SetReceptionAwarenessStep();

                // Act                
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertReceptionAwareness(
                    messageId,
                    awareness =>
                    {
                        Assert.Equal(receptionAwareness.RetryCount, awareness.TotalRetryCount);
                        Assert.Equal(receptionAwareness.RetryInterval, awareness.RetryInterval);
                    });
            }

            private void AssertReceptionAwareness(string messageId, Action<EntityReceptionAwareness> condition)
            {
                using (DatastoreContext context = InMemoryDatastore())
                {
                    EntityReceptionAwareness receptionAwareness =
                        context.ReceptionAwareness.FirstOrDefault(a => a.InternalMessageId.Equals(messageId));

                    Assert.NotNull(receptionAwareness);
                    Assert.Equal(0, receptionAwareness.CurrentRetryCount);
                    Assert.Equal(ReceptionStatus.Pending, receptionAwareness.Status);

                    condition(receptionAwareness);
                }
            }

            private static InternalMessage CreateDefaultInternalMessage(
                string messageId,
                AS4.Model.PMode.ReceptionAwareness receptionAwareness)
            {
                var pmode = new SendingProcessingMode {Reliability = {ReceptionAwareness = receptionAwareness}};
                var userMessage = new UserMessage(messageId);
                var as4Message = new AS4Message {SendingPMode = pmode};
                as4Message.UserMessages.Add(userMessage);

                return new InternalMessage(as4Message);
            }
        }

        protected AS4.Model.PMode.ReceptionAwareness CreatePModeReceptionAwareness()
        {
            return new AS4.Model.PMode.ReceptionAwareness {IsEnabled = true, RetryCount = 3, RetryInterval = "00:05:00"};
        }
    }
}