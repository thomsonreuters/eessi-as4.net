using System;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Steps;
using Microsoft.EntityFrameworkCore;

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
            using (var context = GetDataStoreContext())
            {
                var receipt = new OutMessage { EbmsMessageId = CreateReceipt().MessageId };
                var error = new OutMessage { EbmsMessageId = CreateError().MessageId };

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
        protected Error CreateError()
        {
            return new Error(ErrorMessageId) {RefToMessageId = ErrorMessageId};
        }
    }
}