using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SetReceptionAwarenessStep"/>
    /// </summary>
    public class GivenSetReceptionAwarenessStepFacts : GivenDatastoreFacts
    {
        protected readonly IDatastoreRepository Repository;

        public GivenSetReceptionAwarenessStepFacts()
        {
            this.Repository = new DatastoreRepository(() 
                => new DatastoreContext(base.Options));
        }

        public class GivenValidArguments : GivenSetReceptionAwarenessStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepInsertsReceptionAwarenessAsync()
            {
                // Arrange
                const string messageId = "message-id";
                AS4.Model.PMode.ReceptionAwareness receptionAwareness = base.CreatePModeReceptionAwareness();
                InternalMessage internalMessage = CreateDefaultInternalMessage(messageId, receptionAwareness);
                var step = new SetReceptionAwarenessStep(base.Repository);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertReceptionAwareness(messageId, a =>
                {
                    Assert.Equal(receptionAwareness.RetryCount, a.TotalRetryCount);
                    Assert.Equal(receptionAwareness.RetryInterval, a.RetryInterval);
                });
            }

            private void AssertReceptionAwareness(string messageId, Action<Entities.ReceptionAwareness> condition)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    Entities.ReceptionAwareness receptionAwareness = context.ReceptionAwareness
                        .FirstOrDefault(a => a.InternalMessageId.Equals(messageId));

                    Assert.NotNull(receptionAwareness);
                    Assert.Equal(0, receptionAwareness.CurrentRetryCount);
                    Assert.False(receptionAwareness.IsCompleted);
                    condition(receptionAwareness);
                }
            }

            private InternalMessage CreateDefaultInternalMessage(string messageId, AS4.Model.PMode.ReceptionAwareness receptionAwareness)
            {
                var pmode = new AS4.Model.PMode.SendingProcessingMode
                {
                    Reliability = { ReceptionAwareness = receptionAwareness }
                };
                var userMessage = new UserMessage(messageId: messageId);
                var as4Message = new AS4Message {SendingPMode = pmode};
                as4Message.UserMessages.Add(userMessage);
                var internalMessage = new InternalMessage(as4Message);
                return internalMessage;
            }
        }

        protected AS4.Model.PMode.ReceptionAwareness CreatePModeReceptionAwareness()
        {
            return new AS4.Model.PMode.ReceptionAwareness
            {
                IsEnabled = true,
                RetryCount = 3,
                RetryInterval = "00:05:00"
            };
        }
    }
}
