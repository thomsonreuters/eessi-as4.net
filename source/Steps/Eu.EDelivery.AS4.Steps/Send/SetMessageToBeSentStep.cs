using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    [Description("Confirms that the message is ready to be sent.")]
    [Info("Confirm that the message can be sent.")]
    public class SetMessageToBeSentStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4MessageBodyStore _messageStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetMessageToBeSentStep"/> class.
        /// </summary>
        public SetMessageToBeSentStep()
            : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SetMessageToBeSentStep"/> class.
        /// </summary>
        /// <param name="createContext">The get data store context.</param>
        /// <param name="messageStore">The message store.</param>
        public SetMessageToBeSentStep(Func<DatastoreContext> createContext, IAS4MessageBodyStore messageStore)
        {
            _createContext = createContext;
            _messageStore = messageStore;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            Logger.Info($"[{messagingContext.AS4Message.GetPrimaryMessageId()}] Set the message's Operation = 'ToBeSent' ");

            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new OutMessageService(repository, _messageStore);

                await service.UpdateAS4MessageToBeSent(messagingContext.AS4Message, cancellationToken).ConfigureAwait(false);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return StepResult.Success(messagingContext);
        }
    }
}

