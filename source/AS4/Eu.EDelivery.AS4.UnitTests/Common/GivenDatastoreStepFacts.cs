using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Steps;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// <see cref="GivenDatastoreFacts" /> implementation to implement common <see cref="IStep" /> exercise methods.
    /// </summary>
    public abstract class GivenDatastoreStepFacts : GivenDatastoreFacts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenDatastoreStepFacts" /> class.
        /// </summary>
        protected GivenDatastoreStepFacts()
        {
            ReceiptMessageId = Guid.NewGuid().ToString();
            ErrorMessageId = Guid.NewGuid().ToString();

            SeedDataStore(Options);
        }

        private void SeedDataStore(DbContextOptions<DatastoreContext> options)
        {
            using (var context = new DatastoreContext(options))
            {
                var receipt = new OutMessage { EbmsMessageId = CreateReceipt().MessageId };
                var error = new OutMessage { EbmsMessageId = GetError().MessageId };

                context.OutMessages.Add(receipt);
                context.OutMessages.Add(error);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the common used id to create <see cref="Receipt" /> instances.
        /// </summary>
        protected string ReceiptMessageId { get; }

        /// <summary>
        /// Gets the common used id to create <see cref="Error" /> instances.
        /// </summary>
        protected string ErrorMessageId { get; }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected abstract IStep Step { get; }

        /// <summary>
        /// Create a <see cref="Receipt" /> instance with the specified common id's.
        /// </summary>
        /// <returns></returns>
        protected Receipt CreateReceipt()
        {
            return new Receipt(ReceiptMessageId) {RefToMessageId = ReceiptMessageId};
        }

        /// <summary>
        /// Create a <see cref="Error" /> instance with the specified common id's.
        /// </summary>
        /// <returns></returns>
        protected Error GetError()
        {
            return new Error(ErrorMessageId) {RefToMessageId = ErrorMessageId};
        }

        /// <summary>
        /// Assert the Datastore for the first <paramref name="signalMessage"/> (as Out Message) with a given <paramref name="status"/>.
        /// </summary>
        /// <param name="signalMessage">Signal Message for which the id will be searched.</param>
        /// <param name="status"><see cref="OutStatus"/> type to assert on the searched message.</param>
        /// <returns></returns>
        protected async Task AssertOutMessages(
            MessageUnit signalMessage,
            OutStatus status)
        {
            using (var context = new DatastoreContext(Options))
            {
                OutMessage outMessage = await context.OutMessages
                    .FirstOrDefaultAsync(m => m.EbmsMessageId.Equals(signalMessage.MessageId));

                Assert.NotNull(outMessage);
                Assert.Equal(status, outMessage.Status);
            }
        }

        /// <summary>
        /// Assert the Datastore for the first <paramref name="signalMessage"/> (as In Message) that has the <see cref="InStatus.Received"/>.
        /// </summary>
        /// <param name="signalMessage">Signal Message for which the id will be searched.</param>
        /// <returns></returns>
        protected async Task AssertInMessage(MessageUnit signalMessage)
        {
            using (var context = new DatastoreContext(Options))
            {
                InMessage inMessage = await context.InMessages
                    .FirstOrDefaultAsync(m => m.EbmsMessageId.Equals(signalMessage.MessageId));

                Assert.NotNull(inMessage);
                Assert.Equal(InStatus.Received, inMessage.Status);
            }
        }
    }
}