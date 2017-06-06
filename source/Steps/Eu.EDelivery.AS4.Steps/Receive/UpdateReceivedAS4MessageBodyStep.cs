using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class UpdateReceivedAS4MessageBodyStep : IStep
    {
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateReceivedAS4MessageBodyStep"/> class.
        /// </summary>
        public UpdateReceivedAS4MessageBodyStep() : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateReceivedAS4MessageBodyStep" /> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore" /> that must be used to persist the messagebody content.</param>
        public UpdateReceivedAS4MessageBodyStep(
            Func<DatastoreContext> createDatastoreContext,
            IAS4MessageBodyStore messageBodyStore)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            using (DatastoreContext datastoreContext = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(datastoreContext);

                var service = new InMessageService(repository);

                await service.UpdateAS4MessageForDeliveryAndNotification(messagingContext, _messageBodyStore, cancellationToken);

                await datastoreContext.SaveChangesAsync(cancellationToken);
            }

            return StepResult.Success(messagingContext);
        }        
    }
}
