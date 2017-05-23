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
    public class SendUpdateDataStoreStep : IStep
    {
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyPersister _messageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep" /> class
        /// </summary>
        public SendUpdateDataStoreStep() : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyPersisterProvider) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyPersister"></param>
        public SendUpdateDataStoreStep(Func<DatastoreContext> createDatastoreContext, IAS4MessageBodyPersister messageBodyPersister)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyPersister = messageBodyPersister;
        }

        /// <summary>
        /// Execute the Update DataStore Step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                var inMessageService = new InMessageService(new DatastoreRepository(context));

                await inMessageService.InsertAS4Message(internalMessage.AS4Message, _messageBodyPersister, cancellationToken).ConfigureAwait(false);

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return await StepResult.SuccessAsync(internalMessage);
        }
    }
}