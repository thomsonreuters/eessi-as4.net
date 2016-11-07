using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Repositories;
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
        protected IDatastoreRepository Repository;

        public GivenNotifyUpdateOutExceptionDatastoreStepFacts()
        {
            this.Repository = new DatastoreRepository(
                () => new DatastoreContext(base.Options));
        }

        public class GivenValidArguments : GivenNotifyUpdateOutExceptionDatastoreStepFacts
        {
            [Fact]
            public async Task ThenUpdateDatastoreSucceedsWithValidNotifyMessageAsync()
            {
                // Arrange
                OutException outException = CreateDefaultOutException();
                base.InsertOutException(outException);
                var notifyMessage = new NotifyMessage { MessageInfo = { RefToMessageId = outException.EbmsRefToMessageId } };

                var internalMessage = new InternalMessage(notifyMessage);
                var step = new NotifyUpdateOutExceptionDatastoreStep(base.Repository);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertOutException(outException);
            }

            private OutException CreateDefaultOutException()
            {
                return new OutException
                {
                    EbmsRefToMessageId = "ref-to-message-id",
                    Operation = Operation.ToBeNotified,
                };
            }

            private void AssertOutException(OutException previousOutException)
            {
                using (var context = new DatastoreContext(base.Options))
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
            using (var context = new DatastoreContext(base.Options))
            {
                context.OutExceptions.Add(outException);
                context.SaveChanges();
            }
        }
    }
}