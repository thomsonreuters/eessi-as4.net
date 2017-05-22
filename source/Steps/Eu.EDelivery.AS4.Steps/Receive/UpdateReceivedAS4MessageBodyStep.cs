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
        private readonly IAS4MessageBodyPersister _messageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateReceivedAS4MessageBodyStep"/> class.
        /// </summary>
        public UpdateReceivedAS4MessageBodyStep() : this(Registry.Instance.CreateDatastoreContext, Config.Instance.AS4MessageBodyPersister) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateReceivedAS4MessageBodyStep" /> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyPersister">The <see cref="IAS4MessageBodyPersister" /> that must be used to persist the messagebody content.</param>
        public UpdateReceivedAS4MessageBodyStep(
            Func<DatastoreContext> createDatastoreContext,
            IAS4MessageBodyPersister messageBodyPersister)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyPersister = messageBodyPersister;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="internalMessage"/>.
        /// </summary>
        /// <param name="internalMessage">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            using (DatastoreContext datastoreContext = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(datastoreContext);

                var service = new InMessageService(repository);

                await service.UpdateAS4MessageForDeliveryAndNotification(internalMessage.AS4Message, _messageBodyPersister, cancellationToken);

                await datastoreContext.SaveChangesAsync(cancellationToken);
            }

            return StepResult.Success(internalMessage);
        }        
    }
}
