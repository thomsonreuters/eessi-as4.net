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
    /// Testing <see cref="NotifyUpdateDatastoreStep" />
    /// </summary>
    public class GivenNotifyUpdateDatastoreStepForOutExceptionFacts : GivenDatastoreFacts
    {
        public class GivenValidArguments : GivenNotifyUpdateDatastoreStepForOutExceptionFacts
        {
            [Fact]
            public async Task ThenUpdateDatastoreSucceedsWithValidNotifyMessageAsync()
            {
                // Arrange
                OutException outException = CreateDefaultOutException();
                InsertOutException(outException);
                MessagingContext messagingContext = CreateNotifyMessage(outException);

                var step = new NotifyUpdateDatastoreStep();

                // Act
                await step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                GetDataStoreContext.AssertOutException(
                    outException.EbmsRefToMessageId,
                    ex => Assert.Equal(Operation.Notified, ex.Operation));
            }

            private static OutException CreateDefaultOutException()
            {
                return new OutException { EbmsRefToMessageId = "ref-to-message-id", Operation = Operation.ToBeNotified };
            }

            private static MessagingContext CreateNotifyMessage(OutException outException)
            {
                var notifyMessage =
                    new NotifyMessageEnvelope(
                        new MessageInfo { RefToMessageId = outException.EbmsRefToMessageId },
                        Status.Delivered,
                        null,
                        string.Empty,
                        outException.GetType());

                return new MessagingContext(notifyMessage);
            }
        }

        protected void InsertOutException(OutException outException)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                context.OutExceptions.Add(outException);
                context.SaveChanges();
            }
        }
    }
}