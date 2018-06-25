using System;
using System.ComponentModel;
using System.Threading;
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
            : this(Config.Instance, Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

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
            Logger.Info($"{messagingContext.LogTag} Store the incoming AS4 Message to the datastore");

            if (messagingContext.ReceivedMessage == null)
            {
                throw new InvalidOperationException(
                    $"{messagingContext.LogTag} {nameof(SaveReceivedMessageStep)} " + 
                    "requires a ReceivedStream to store the incoming message into the datastore");
            }

            MessagingContext resultContext = await InsertReceivedAS4MessageAsync(messagingContext);

            if (resultContext != null && resultContext.Exception == null)
            {
                if (resultContext.AS4Message.IsSignalMessage
                    && String.IsNullOrWhiteSpace(resultContext.AS4Message.FirstSignalMessage.RefToMessageId))
                {
                    Logger.Warn(
                        $"{messagingContext.LogTag} The received message is a SignalMessage without RefToMessageId. " +
                        "No such SignalMessage are supported so the message cannot be processed any further");

                    return StepResult
                        .Success(new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive))
                        .AndStopExecution();
                }

                Logger.Debug($"{messagingContext.LogTag} The AS4 Message is successfully stored into the datastore");
                return StepResult.Success(resultContext);
            }

            Logger.Error(
                $"{messagingContext.LogTag} The AS4 Message is not stored " + 
                $"correctly into the datastore {resultContext?.Exception}");

            return StepResult.Failed(resultContext);
        }

        private async Task<MessagingContext> InsertReceivedAS4MessageAsync(MessagingContext messagingContext)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                var service = new InMessageService(_config, new DatastoreRepository(context));
                MessageExchangePattern mep = DetermineMessageExchangePattern(messagingContext);

                MessagingContext resultContext = await service
                    .InsertAS4MessageAsync(messagingContext, mep, _messageBodyStore)
                    .ConfigureAwait(false);

                await context.SaveChangesAsync().ConfigureAwait(false);

                return resultContext;
            }
        }

        private static MessageExchangePattern DetermineMessageExchangePattern(MessagingContext messagingContext)
        {
            if (messagingContext.SendingPMode?.MepBinding == MessageExchangePatternBinding.Pull)
            {
                return MessageExchangePattern.Pull;
            }

            return MessageExchangePattern.Push;
        }
    }
}