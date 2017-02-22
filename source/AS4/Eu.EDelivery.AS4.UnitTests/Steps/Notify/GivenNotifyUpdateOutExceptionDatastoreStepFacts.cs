using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="NotifyUpdateOutExceptionDatastoreStep"/>
    /// </summary>
    public class GivenNotifyUpdateOutExceptionDatastoreStepFacts : GivenDatastoreFacts
    {
        public class GivenValidArguments : GivenNotifyUpdateOutExceptionDatastoreStepFacts
        {
            [Fact]
            public async Task ThenUpdateDatastoreSucceedsWithValidNotifyMessageAsync()
            {
                // Arrange
                OutException outException = CreateDefaultOutException();
                base.InsertOutException(outException);
                var notifyMessage = new NotifyMessageEnvelope(new MessageInfo() { RefToMessageId = outException.EbmsRefToMessageId }, Status.Delivered, null, string.Empty);

                var internalMessage = new InternalMessage(notifyMessage);
                var step = new NotifyUpdateOutExceptionDatastoreStep();

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                AssertOutException(outException);
            }

            private static OutException CreateDefaultOutException()
            {
                return new OutException
                {
                    EbmsRefToMessageId = "ref-to-message-id",
                    Operation = Operation.ToBeNotified,
                };
            }

            private void AssertOutException(OutException previousOutException)
            {
                using (var context = GetDataStoreContext())
                {
                    OutException outException = context.OutExceptions
                        .FirstOrDefault(e => e.EbmsRefToMessageId.Equals(previousOutException.EbmsRefToMessageId));

                    Assert.NotNull(outException);
                    Assert.Equal(Operation.Notified, outException.Operation);
                }
            }
        }

        protected void InsertOutException(OutException outException)
        {
            using (var context = GetDataStoreContext())
            {
                context.OutExceptions.Add(outException);
                context.SaveChanges();
            }
        }
    }
}