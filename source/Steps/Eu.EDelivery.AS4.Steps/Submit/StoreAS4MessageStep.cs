using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Describes how the AS4 UserMessage is stored in the message store,
    /// in order to hand over to the Send Agents.
    /// </summary>
    [Info("Store AS4 message")]
    [Description(
        "Stores the AS4 Message that has been created for the received SubmitMessage " + 
        "so that it can be processed (signed, encrypted, …) afterwards.")]
    public class StoreAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // TODO: this class should be reviewed IMHO.  We should not save AS4Messages, but we should
        // save the MessagePart in the OutMessage table.  Each MessagePart has its own messagebody.
        // Right now, the MessageBody is the complete AS4Message; every OutMessage refers to that same messagebody which 
        // is not correct.
        // At this stage, there should be no AS4-message in my opinion, only UserMessages and SignalMessages.
        private readonly IConfig _config;
        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep" /> class.
        /// </summary>
        public StoreAS4MessageStep()
            : this(Config.Instance, Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="createContext">The create context.</param>
        /// <param name="messageBodyStore">The as 4 Message Body Persister.</param>
        public StoreAS4MessageStep(
            IConfig configuration,
            Func<DatastoreContext> createContext, 
            IAS4MessageBodyStore messageBodyStore)
        {
            _config = configuration;
            _createContext = createContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">The Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            Logger.Trace("Storing the AS4Message with Operation=ToBeProcessed");

            using (DatastoreContext context = _createContext())
            {
                var service = new OutMessageService(_config, new DatastoreRepository(context), _messageBodyStore);

                service.InsertAS4Message(messagingContext, Operation.ToBeProcessed);

                try
                {
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
                catch
                {
                    messagingContext.ErrorResult = new ErrorResult(
                        "Unable to store the received message due to an exception", 
                        ErrorAlias.Other);

                    throw;
                }
            }

            Logger.Info(
                $"{messagingContext.LogTag} Stored the AS4Message with Operation=ToBeProcesed so the next agent can handle the message");

            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}