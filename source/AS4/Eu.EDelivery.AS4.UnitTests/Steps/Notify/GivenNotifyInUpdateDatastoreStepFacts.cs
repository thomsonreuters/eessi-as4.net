using System.Threading;
using System.Threading.Tasks;
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
    /// Testing <see cref="NotifyUpdateDatastoreStep" />
    /// </summary>
    public class GivenNotifyUpdateDatastoreStepForInMessageFacts : GivenDatastoreFacts
    {
        private readonly NotifyUpdateDatastoreStep _step;

        public GivenNotifyUpdateDatastoreStepForInMessageFacts()
        {
            _step = new NotifyUpdateDatastoreStep();
        }

        private static InMessage CreateInMessage(string id)
        {
            return new InMessage { EbmsMessageId = id };
        }

        public class GivenValidArguments : GivenNotifyUpdateDatastoreStepForInMessageFacts
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
                GetDataStoreContext.AssertInMessage(
                    notifyMessage.MessageInfo.MessageId,
                    m =>
                    {
                        Assert.Equal(Operation.Notified, OperationUtils.Parse(m.Operation));
                        Assert.Equal(InStatus.Notified, InStatusUtils.Parse(m.Status));
                    });
            }

            private void InsertDefaultInMessage(string sharedId)
            {
                InMessage inMessage = CreateInMessage(sharedId);
                inMessage.SetOperation(Operation.Notifying);
                inMessage.SetStatus(InStatus.Delivered);

                GetDataStoreContext.InsertInMessage(inMessage);
            }

            private static NotifyMessageEnvelope CreateNotifyMessage(string id)
            {
                var msgInfo = new MessageInfo { MessageId = id };

                return new NotifyMessageEnvelope(msgInfo, Status.Delivered, null, string.Empty, typeof(InMessage));
            }
        }
    }
}