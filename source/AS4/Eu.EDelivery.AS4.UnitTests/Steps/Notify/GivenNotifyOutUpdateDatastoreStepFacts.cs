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
    /// Testing <see cref="NotifyUpdateInMessageDatastoreStep"/>
    /// </summary>
    public class GivenNotifyOutUpdateDatastoreStepFacts : GivenDatastoreFacts
    {
        private readonly NotifyUpdateOutMessageDatastoreStep _step;

        public GivenNotifyOutUpdateDatastoreStepFacts()
        {
            this._step = new NotifyUpdateOutMessageDatastoreStep();
        }

        public class GivenValidArguments : GivenNotifyOutUpdateDatastoreStepFacts
        {
            [Theory, InlineData("shared-id")]
            public async Task ThenExecuteStepSucceedsWithValidNotifyMessageAsync(string sharedId)
            {
                // Arrange
                InsertDefaultOutMessage(sharedId);
                NotifyMessage notifyMessage = CreateNotifyMessage(sharedId);
                var internalMessage = new InternalMessage(notifyMessage);
                // Act
                await base._step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertOutMessage(notifyMessage.MessageInfo.MessageId, m =>
                {
                    Assert.Equal(Operation.Notified, m.Operation);
                    Assert.Equal(OutStatus.Notified, m.Status);
                });
            }

            private void InsertDefaultOutMessage(string sharedId)
            {
                OutMessage outMessage = CreateOutMessage(sharedId);
                outMessage.Operation = Operation.Notifying;
                outMessage.Status = OutStatus.Ack;
                base.InsertOutMessage(outMessage);
            }

            private static NotifyMessage CreateNotifyMessage(string id)
            {
                return new NotifyMessage { MessageInfo = { MessageId = id } };
            }

            private void AssertOutMessage(string messageId, Action<OutMessage> assertAction)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    OutMessage outMessage = context.OutMessages
                        .FirstOrDefault(m => m.EbmsMessageId.Equals(messageId));
                    assertAction(outMessage);
                }
            }
        }

        protected void InsertOutMessage(OutMessage outMessage)
        {
            using (var context = new DatastoreContext(base.Options))
            {
                context.OutMessages.Add(outMessage);
                context.SaveChanges();
            }
        }

        private static OutMessage CreateOutMessage(string id)
        {
            return new OutMessage { EbmsMessageId = id };
        }
    }
}