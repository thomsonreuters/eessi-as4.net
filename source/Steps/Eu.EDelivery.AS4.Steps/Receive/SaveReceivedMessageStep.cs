using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is received.
    /// </summary>
    public class SaveReceivedMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyPersister _messageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveReceivedMessageStep" /> class
        /// </summary>
        public SaveReceivedMessageStep() : this(Registry.Instance.CreateDatastoreContext, Config.Instance.IncomingAS4MessageBodyPersister) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveReceivedMessageStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyPersister">The <see cref="IAS4MessageBodyPersister"/> that must be used to persist the messagebody content.</param>
        public SaveReceivedMessageStep(Func<DatastoreContext> createDatastoreContext, IAS4MessageBodyPersister messageBodyPersister)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyPersister = messageBodyPersister;
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken token)
        {
            Logger.Info($"{internalMessage.Prefix} Update Datastore with AS4 received message");

            using (DatastoreContext context = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new InMessageService(repository);

                await service.InsertAS4Message(internalMessage, _messageBodyPersister, token).ConfigureAwait(false);
                await context.SaveChangesAsync(token).ConfigureAwait(false);
            }

            return StepResult.Success(internalMessage);
        }
    }
}