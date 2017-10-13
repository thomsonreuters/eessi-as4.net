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
    [Description("Saves a received message as-is in the datastore.")]
    [Info("Save received message")]
    public class SaveReceivedMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveReceivedMessageStep" /> class
        /// </summary>
        public SaveReceivedMessageStep() : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveReceivedMessageStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore"/> that must be used to persist the messagebody content.</param>
        public SaveReceivedMessageStep(Func<DatastoreContext> createDatastoreContext, IAS4MessageBodyStore messageBodyStore)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken token)
        {
            Logger.Info($"{messagingContext.EbmsMessageId} Insert received message in datastore");

            if (messagingContext.ReceivedMessage == null)
            {
                throw new InvalidOperationException("SaveReceivedMessageStep requires a ReceivedStream");
            }

            using (DatastoreContext context = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new InMessageService(repository);

                var mep = DetermineMessageExchangePattern(messagingContext);

                var resultContext = await service.InsertAS4Message(messagingContext, mep, _messageBodyStore, token).ConfigureAwait(false);
                await context.SaveChangesAsync(token).ConfigureAwait(false);

                if (resultContext != null && resultContext.Exception == null)
                {                    
                    if (resultContext.AS4Message.IsSignalMessage &&
                        String.IsNullOrWhiteSpace(resultContext.AS4Message.PrimarySignalMessage.RefToMessageId))
                    {
                        Logger.Info("The received message is a signal-message without RefToMessageId.  It cannot be processed any further.");
                        return StepResult.Success(new MessagingContext(AS4Message.Empty, MessagingContextMode.Receive)).AndStopExecution();
                    }

                    return StepResult.Success(resultContext);
                }
                else
                {
                    return StepResult.Failed(resultContext);
                }
            }
        }

        private static MessageExchangePattern DetermineMessageExchangePattern(MessagingContext messagingContext)
        {
            if (messagingContext.SendingPMode == null)
            {
                return MessageExchangePattern.Push;
            }

            if (messagingContext.SendingPMode.MepBinding == MessageExchangePatternBinding.Pull)
            {
                return MessageExchangePattern.Pull;
            }

            return MessageExchangePattern.Push;
        }
    }
}