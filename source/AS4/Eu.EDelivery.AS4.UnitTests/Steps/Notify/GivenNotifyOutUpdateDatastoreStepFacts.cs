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
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="NotifyUpdateInMessageDatastoreStep" />
    /// </summary>
    public class GivenNotifyOutUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        private readonly NotifyUpdateOutMessageDatastoreStep _step;

        public GivenNotifyOutUpdateDatastoreStepFacts()
        {
            _step = new NotifyUpdateOutMessageDatastoreStep();
        }

        public class GivenValidArguments : GivenNotifyOutUpdateDatastoreStepFacts
        {
            [Theory]
            [InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithValidNotifyMessageAsync(string sharedId)
            {
                // Arrange
                InsertDefaultOutMessage(sharedId);
                NotifyMessageEnvelope notifyMessage = CreateNotifyMessage(sharedId);
                var internalMessage = new MessagingContext(notifyMessage);

                // Act
                await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertOutMessage(
                    notifyMessage.MessageInfo.MessageId,
                    m =>
                    {
                        Assert.Equal(Operation.Notified, m.Operation);
                        Assert.Equal(OutStatus.Notified, m.Status);
                    });
            }

            private void InsertDefaultOutMessage(string sharedId)
            {
                var outMessage = new OutMessage
                {
                    EbmsMessageId = sharedId,
                    Operation = Operation.Notifying,
                    Status = OutStatus.Ack
                };
                GetDataStoreContext.InsertOutMessage(outMessage);
            }

            private static NotifyMessageEnvelope CreateNotifyMessage(string id)
            {
                var msgInfo = new MessageInfo {MessageId = id};

                return new NotifyMessageEnvelope(msgInfo, Status.Delivered, null, string.Empty);
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> assertAction)
            {
                using (var context = new DatastoreContext(Options))
                {
                    OutMessage outMessage = context.OutMessages.FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(outMessage);
                }
            }
        }
    }
}