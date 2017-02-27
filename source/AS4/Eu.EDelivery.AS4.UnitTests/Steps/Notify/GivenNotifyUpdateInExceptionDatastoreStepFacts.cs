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
    /// Testing <see cref="NotifyUpdateInExceptionDatastoreStep"/> 
    /// </summary>
    public class GivenNotifyUpdateInExceptionDatastoreStepFacts : GivenDatastoreFacts
    {

        public class GivenValidArguments : GivenNotifyUpdateInExceptionDatastoreStepFacts
        {
            [Fact]
            public async Task ThenUpdateDatastoreSucceedsWithValidNotifyMessageAsync()
            {
                // Arrange
                InException inException = CreateDefaultInException();
                base.InsertInException(inException);


                var msgInfo = new MessageInfo() { RefToMessageId = inException.EbmsRefToMessageId };

                var notifyMessage = new NotifyMessageEnvelope(msgInfo, Status.Delivered, null, String.Empty);


                var internalMessage = new InternalMessage(notifyMessage);
                var step = new NotifyUpdateInExceptionDatastoreStep();

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

            private static InException CreateDefaultInException()
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