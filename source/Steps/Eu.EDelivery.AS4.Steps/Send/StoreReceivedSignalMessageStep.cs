using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Updates the DataStore with the response that was received synchronously after sending an AS4Message.
    /// </summary>
    [Obsolete("This Step can be replaced by the regular SaveReceivedMessageStep")]
    public class StoreReceivedSignalMessageStep : IStep
    {
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreReceivedSignalMessageStep" /> class
        /// </summary>
        public StoreReceivedSignalMessageStep() : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreReceivedSignalMessageStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyStore"></param>
        public StoreReceivedSignalMessageStep(Func<DatastoreContext> createDatastoreContext, IAS4MessageBodyStore messageBodyStore)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Execute the Update DataStore Step
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                var inMessageService = new InMessageService(new DatastoreRepository(context));

                await inMessageService.InsertAS4Message(messagingContext, _messageBodyStore, cancellationToken).ConfigureAwait(false);

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}