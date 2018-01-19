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
    /// Testing <see cref="NotifyUpdateDatastoreStep" />
    /// </summary>
    public class GivenNotifyUpdateDatastoreStepForInExceptionsFacts : GivenDatastoreFacts
    {
        public class GivenValidArguments : GivenNotifyUpdateDatastoreStepForInExceptionsFacts
        {
            [Fact]
            public async Task ThenUpdateDatastoreSucceedsWithValidNotifyMessageAsync()
            {
                // Arrange
                InException inException = CreateDefaultInException();
                InsertInException(inException);
                MessagingContext messagingContext = CreateNotifyMessage(inException);
                var step = new NotifyUpdateDatastoreStep();

                // Act
                await step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertInException(inException);
            }

            private static InException CreateDefaultInException()
            {
                var exception = new InException("ref-to-message-id", "");

                exception.SetOperation(Operation.ToBeNotified);

                return exception;
            }

            private static MessagingContext CreateNotifyMessage(ExceptionEntity inException)
            {
                var msgInfo = new MessageInfo { RefToMessageId = inException.EbmsRefToMessageId };

                var notifyMessage = new NotifyMessageEnvelope(msgInfo, Status.Delivered, null, string.Empty, inException.GetType());

                return new MessagingContext(notifyMessage);
            }

            private void AssertInException(ExceptionEntity previousInException)
            {
                using (var context = GetDataStoreContext())
                {
                    InException inException =
                        context.InExceptions.FirstOrDefault(
                            e => e.EbmsRefToMessageId.Equals(previousInException.EbmsRefToMessageId));

                    Assert.NotNull(inException);
                    Assert.Equal(Operation.Notified, OperationUtils.Parse(inException.Operation));
                }
            }
        }

        protected void InsertInException(InException inException)
        {
            using (var context = GetDataStoreContext())
            {
                context.InExceptions.Add(inException);
                context.SaveChanges();
            }
        }
    }
}