using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="DeliverUpdateDatastoreStep" />
    /// </summary>
    public class GivenDeliverUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        private readonly string _messageId;
        private readonly DeliverUpdateDatastoreStep _step;

        public GivenDeliverUpdateDatastoreStepFacts()
        {
            _messageId = "message-id";
            _step = new DeliverUpdateDatastoreStep();

            SeedDatastore();
        }

        private void SeedDatastore()
        {
            using (var context = new DatastoreContext(Options))
            {
                InMessage inMessage = CreateInMessage();
                context.InMessages.Add(inMessage);
                context.SaveChanges();
            }
        }

        private InMessage CreateInMessage()
        {
            var inMessage = new InMessage
            {
                EbmsMessageId = _messageId
            };

            inMessage.SetStatus(InStatus.Received);
            inMessage.SetOperation(Operation.ToBeDelivered);

            return inMessage;
        }

        public class GivenValidArguments : GivenDeliverUpdateDatastoreStepFacts
        {
            [Fact]
            public async Task ThenExecuteMethodSucceedsWithValidUserMessageAsync()
            {
                // Arrange
                MessagingContext messagingContext = CreateDefaultInternalMessage();

                // Act
                await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertInMessages();
            }

            private void AssertInMessages()
            {
                using (var context = new DatastoreContext(Options))
                {
                    InMessage inmessage = context.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(_messageId));

                    Assert.NotNull(inmessage);
                    Assert.Equal(InStatus.Delivered, InStatusUtils.Parse(inmessage.Status));
                    Assert.Equal(Operation.Delivered, OperationUtils.Parse(inmessage.Operation));
                }
            }
        }

        protected DeliverMessageEnvelope CreateDeliverMessage()
        {
            return new DeliverMessageEnvelope(new MessageInfo { MessageId = _messageId }, new byte[] { }, string.Empty);
        }

        protected MessagingContext CreateDefaultInternalMessage()
        {
            return new MessagingContext(CreateDeliverMessage());
        }
    }
}