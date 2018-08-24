using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is received.
    /// </summary>
    [Info("Save received message")]
    [Description("Saves a received message as-is in the datastore.")]
    public class SaveReceivedMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IConfig _config;
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveReceivedMessageStep" /> class
        /// </summary>
        public SaveReceivedMessageStep() 
            : this(
                Config.Instance, 
                Registry.Instance.CreateDatastoreContext, 
                Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveReceivedMessageStep"/> class.
        /// </summary>
        /// <param name="configuration">The configuration</param>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore"/> that must be used to persist the messagebody content.</param>
        public SaveReceivedMessageStep(
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

            _config = configuration;
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext?.ReceivedMessage == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SaveReceivedMessageStep)} requires a ReceivedMessage to store the incoming message into the datastore but no ReceivedMessage is present in the MessagingContext");
            }

            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SaveReceivedMessageStep)} requires an AS4Message to save but no AS4Message is present in the MessagingContext");
            }

            Logger.Info($"{messagingContext.LogTag} Store the incoming AS4 Message to the datastore");
            MessagingContext resultContext = await InsertReceivedAS4MessageAsync(messagingContext);

            if (messagingContext.AS4Message.PrimaryMessageUnit is Error signal
                && signal.RefToMessageId == null)
            {
                Logger.Warn($"{messagingContext.LogTag} cannot further process incoming Error because it hasn't got a RefToMessageId");
                
                return StepResult.Success(
                    new MessagingContext(AS4Message.Empty, messagingContext.Mode))
                                 .AndStopExecution();
            }

            if (resultContext != null && resultContext.Exception == null)
            {
                if (resultContext.AS4Message.IsSignalMessage
                    && String.IsNullOrWhiteSpace(resultContext.AS4Message.FirstSignalMessage.RefToMessageId))
                {
                    Logger.Warn(
                        $"{messagingContext.LogTag} Received message is a SignalMessage without RefToMessageId. " +
                        "No such SignalMessage are supported so the message cannot be processed any further");

                    return StepResult
                        .Success(new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive))
                        .AndStopExecution();
                }

                Logger.Debug($"{messagingContext.LogTag} The AS4Message is successfully stored into the datastore");
                return StepResult.Success(resultContext);
            }

            Logger.Error(
                $"{messagingContext.LogTag} The AS4Message is not stored correctly into the datastore {resultContext?.Exception}");

            return StepResult.Failed(resultContext);
        }

        private async Task<MessagingContext> InsertReceivedAS4MessageAsync(MessagingContext messagingContext)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                MessageExchangePattern mep =
                    messagingContext.SendingPMode?.MepBinding == MessageExchangePatternBinding.Pull
                        ? MessageExchangePattern.Pull
                        : MessageExchangePattern.Push;

                var service = new InMessageService(_config, new DatastoreRepository(context));
                MessagingContext resultContext = await service
                    .InsertAS4MessageAsync(messagingContext, mep, _messageBodyStore)
                    .ConfigureAwait(false);

                await context.SaveChangesAsync().ConfigureAwait(false);

                return resultContext;
            }
        }
    }
}