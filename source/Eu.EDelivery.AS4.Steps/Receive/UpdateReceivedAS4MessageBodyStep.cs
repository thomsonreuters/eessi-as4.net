using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    [Info("Update the received AS4 Message")]
    [Description("Updates the AS4 Message that has been received after processing so that it can be delivered or forwarded")]
    public class UpdateReceivedAS4MessageBodyStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IConfig _configuration;
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateReceivedAS4MessageBodyStep"/> class.
        /// </summary>
        public UpdateReceivedAS4MessageBodyStep() 
            : this(
                Config.Instance,
                Registry.Instance.CreateDatastoreContext, 
                Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateReceivedAS4MessageBodyStep" /> class.
        /// </summary>
        /// <param name="configuration">The configuration used to save the message</param>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore" /> that must be used to persist the messagebody content.</param>
        public UpdateReceivedAS4MessageBodyStep(
            IConfig configuration,
            Func<DatastoreContext> createDatastoreContext,
            IAS4MessageBodyStore messageBodyStore)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (createDatastoreContext == null)
            {
                throw new ArgumentNullException(nameof(createDatastoreContext));
            }

            if (messageBodyStore == null)
            {
                throw new ArgumentNullException(nameof(messageBodyStore));
            }

            _configuration = configuration;
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(UpdateReceivedAS4MessageBodyStep)} requires an AS4Message to update but no AS4Message is present in the MessagingContext");
            }

            Logger.Trace("Updating the received message body...");
            using (DatastoreContext datastoreContext = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(datastoreContext);
                var service = new InMessageService(_configuration, repository);

                service.UpdateAS4MessageForMessageHandling(
                    messagingContext,
                    _messageBodyStore);

                await datastoreContext.SaveChangesAsync().ConfigureAwait(false);
            }

            if (messagingContext.ReceivedMessageMustBeForwarded)
            {
                // When the Message has to be forwarded, the remaining Steps must not be executed.
                // The MSH must answer with a HTTP Accepted status-code, so an empty context must be returned.
                messagingContext.ModifyContext(AS4Message.Empty);

                Logger.Info(
                    "Stops execution to return empty SOAP envelope to the orignal sender. " +
                    "This happens when the message must be forwarded");

                return StepResult.Success(messagingContext).AndStopExecution();
            }

            return StepResult.Success(messagingContext);
        }
    }
}
