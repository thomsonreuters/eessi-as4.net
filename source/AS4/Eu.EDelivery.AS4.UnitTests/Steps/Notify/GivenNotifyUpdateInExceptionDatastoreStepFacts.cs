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
    /// Testing <see cref="NotifyUpdateInExceptionDatastoreStep" />
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
                InsertInException(inException);
                MessagingContext messagingContext = CreateNotifyMessage(inException);
                var step = new NotifyUpdateInExceptionDatastoreStep();

                // Act
                await step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertInException(inException);
            }

            private static InException CreateDefaultInException()
            {
                return new InException {EbmsRefToMessageId = "ref-to-message-id", Operation = Operation.ToBeNotified};
            }

            private static MessagingContext CreateNotifyMessage(ExceptionEntity inException)
            {
                var msgInfo = new MessageInfo {RefToMessageId = inException.EbmsRefToMessageId};

                var notifyMessage = new NotifyMessageEnvelope(msgInfo, Status.Delivered, null, string.Empty);

                return new MessagingContext(notifyMessage);
            }

            private void AssertInException(ExceptionEntity previousInException)
            {
                using (var context = new DatastoreContext(Options))
                {
                    InException inException =
                        context.InExceptions.FirstOrDefault(
                            e => e.EbmsRefToMessageId.Equals(previousInException.EbmsRefToMessageId));

                    Assert.NotNull(inException);
                    Assert.Equal(Operation.Notified, inException.Operation);
                }
            }
        }

        protected void InsertInException(InException inException)
        {
            using (var context = new DatastoreContext(Options))
            {
                context.InExceptions.Add(inException);
                context.SaveChanges();
            }
        }
    }
}