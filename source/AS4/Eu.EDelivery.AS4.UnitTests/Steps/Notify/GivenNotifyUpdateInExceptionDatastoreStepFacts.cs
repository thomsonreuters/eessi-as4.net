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
    /// Testing <see cref="NotifyUpdateInExceptionDatastoreStep"/> 
    /// </summary>
    public class GivenNotifyUpdateInExceptionDatastoreStepFacts : GivenDatastoreFacts
    {
        protected IDatastoreRepository Repository;

        public GivenNotifyUpdateInExceptionDatastoreStepFacts()
        {
            this.Repository = new DatastoreRepository(
                () => new DatastoreContext(base.Options));
        }

        public class GivenValidArguments : GivenNotifyUpdateInExceptionDatastoreStepFacts
        {
            [Fact]
            public async Task ThenUpdateDatastoreSucceedsWithValidNotifyMessageAsync()
            {
                // Arrange
                InException inException = CreateDefaultInException();
                base.InsertInException(inException);

                var notifyMessage = new NotifyMessage {MessageInfo = {RefToMessageId = inException.EbmsRefToMessageId}};

                var internalMessage = new InternalMessage(notifyMessage);
                var step = new NotifyUpdateInExceptionDatastoreStep(base.Repository);

                // Act
                await step.ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                AssertInException(inException);
            }

            private void AssertInException(InException previousInException)
            {
                using (var context = new DatastoreContext(base.Options))
                {
                    InException inException = context.InExceptions
                        .FirstOrDefault(e => e.EbmsRefToMessageId.Equals(previousInException.EbmsRefToMessageId));

                    Assert.NotNull(inException);
                    Assert.Equal(Operation.Notified, inException.Operation);
                }
            }

            private InException CreateDefaultInException()
            {
                return new InException
                {
                    EbmsRefToMessageId = "ref-to-message-id",
                    Operation = Operation.ToBeNotified,
                };
            }
        }

        protected void InsertInException(InException inException)
        {
            using (var context = new DatastoreContext(base.Options))
            {
                context.InExceptions.Add(inException);
                context.SaveChanges();
            }
        }
    }
}