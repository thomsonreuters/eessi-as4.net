using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="DeliverUpdateDatastoreStep"/>
    /// </summary>
    public class GivenDeliverUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        private DeliverUpdateDatastoreStep _step;
        private readonly string _messageId;

        public GivenDeliverUpdateDatastoreStepFacts()
        {
            this._messageId = "message-id";
            this._step = new DeliverUpdateDatastoreStep(
                new DatastoreRepository(() => new DatastoreContext(base.Options)));

            SeedDatastore();
        }

        private void SeedDatastore()
        {
            using (var context = new DatastoreContext(base.Options))
            {
                InMessage inMessage = CreateInMessage();
                context.InMessages.Add(inMessage);
                context.SaveChanges();
            }
        }

        private InMessage CreateInMessage()
        {
            return new InMessage
            {
                EbmsMessageId = this._messageId,
                Status = InStatus.Received,
                Operation = Operation.ToBeDelivered
            };
        }

        public class GivenValidArguments : GivenDeliverUpdateDatastoreStepFacts
        {
            [Fact]
            public async Task ThenExecuteMethodSucceedsWithValidUserMessageAsync()
            {
                // Arrange
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();
                // Act
                StepResult result = await base._step
                    .ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertInMessages();
            }

            private void AssertInMessages()
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    InMessage inmessage = context.InMessages
                        .FirstOrDefault(m => m.EbmsMessageId.Equals(this._messageId));

                    Assert.NotNull(inmessage);
                    Assert.Equal(InStatus.Delivered, inmessage.Status);
                    Assert.Equal(Operation.Delivered, inmessage.Operation);
                }
            }
        }

        protected DeliverMessage CreateDeliverMessage()
        {
            return new DeliverMessage {MessageInfo = {MessageId = this._messageId}};
        }

        protected InternalMessage CreateDefaultInternalMessage()
        {
            return new InternalMessage(CreateDeliverMessage());
        }
    }
}