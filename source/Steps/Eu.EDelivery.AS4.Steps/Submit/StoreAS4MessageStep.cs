using System;
using System.ComponentModel;
using System.Threading;
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
    [Description("Stores the AS4 Message that has been created for the received submit-message so that it can be processed (signed, encrypted, …) afterwards.")]
    [Info("Store AS4 message")]
    public class StoreAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // TODO: this class should be reviewed IMHO.  We should not save AS4Messages, but we should
        // save the MessagePart in the OutMessage table.  Each MessagePart has its own messagebody.
        // Right now, the MessageBody is the complete AS4Message; every OutMessage refers to that same messagebody which 
        // is not correct.
        // At this stage, there should be no AS4-message in my opinion, only UserMessages and SignalMessages.
        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep" /> class.
        /// </summary>
        public StoreAS4MessageStep()
            : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="messageBodyStore">The as 4 Message Body Persister.</param>
        public StoreAS4MessageStep(Func<DatastoreContext> createContext, IAS4MessageBodyStore messageBodyStore)
        {
            _createContext = createContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">The Message used during the step execution.</param>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellation)
        {
            Logger.Info($"[{messagingContext.AS4Message.GetPrimaryMessageId()}] Storing the AS4 Message with Operation = 'ToBeProcessed'");

            using (DatastoreContext context = _createContext())
            {
                var service = new OutMessageService(new DatastoreRepository(context), _messageBodyStore);

                await service.InsertAS4MessageAsync(messagingContext, Operation.ToBeProcessed, cancellation).ConfigureAwait(false);

                try
                {
                    await context.SaveChangesAsync(cancellation).ConfigureAwait(false);
                }
                catch
                {
                    messagingContext.ErrorResult = new ErrorResult("Unable to store the received message.", ErrorAlias.Other);
                    throw;
                }
            }

            Logger.Info($"[{messagingContext.AS4Message.GetPrimaryMessageId()}] Stored the AS4 Message");

            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}