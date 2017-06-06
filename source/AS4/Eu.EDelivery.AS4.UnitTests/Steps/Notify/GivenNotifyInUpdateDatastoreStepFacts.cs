using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="NotifyUpdateInMessageDatastoreStep" />
    /// </summary>
    public class GivenNotifyInUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        private readonly NotifyUpdateInMessageDatastoreStep _step;

        public GivenNotifyInUpdateDatastoreStepFacts()
        {
            _step = new NotifyUpdateInMessageDatastoreStep();
        }

        private static InMessage CreateInMessage(string id)
        {
            return new InMessage {EbmsMessageId = id};
        }

        public class GivenValidArguments : GivenNotifyInUpdateDatastoreStepFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithValidNotifyMessageAsync(string sharedId)
            {
                // Arrange
                InsertDefaultInMessage(sharedId);
                NotifyMessageEnvelope notifyMessage = CreateNotifyMessage(sharedId);
                var internalMessage = new MessagingContext(notifyMessage);

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertInMessage(
                    notifyMessage.MessageInfo.MessageId,
                    m =>
                    {
                        Assert.Equal(Operation.Notified, m.Operation);
                        Assert.Equal(InStatus.Notified, m.Status);
                    });
            }

            private void InsertDefaultInMessage(string sharedId)
            {
                InMessage inMessage = CreateInMessage(sharedId);
                inMessage.Operation = Operation.Notifying;
                inMessage.Status = InStatus.Delivered;
                InsertInMessage(inMessage);
            }

            private static NotifyMessageEnvelope CreateNotifyMessage(string id)
            {
                var msgInfo = new MessageInfo {MessageId = id};

                return new NotifyMessageEnvelope(msgInfo, Status.Delivered, null, string.Empty);
            }

            private void AssertInMessage(string messageId, Action<InMessage> assertAction)
            {
                using (var context = new DatastoreContext(Options))
                {
                    InMessage inMessage = context.InMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(inMessage);
                }
            }
        }

        protected void InsertInMessage(InMessage inMessage)
        {
            using (var context = new DatastoreContext(Options))
            {
                context.InMessages.Add(inMessage);
                context.SaveChanges();
            }
        }
    }
}