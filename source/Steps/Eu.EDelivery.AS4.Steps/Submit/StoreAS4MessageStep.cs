using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Describes how the AS4 UserMessage is stored in the message store,
    /// in order to hand over to the Send Agents.
    /// </summary>
    public class StoreAS4MessageStep : IStep
    {
        // TODO: this class should be reviewed IMHO.  We should not save AS4Messages, but we should
        // save the MessagePart in the OutMessage table.  Each MessagePart has its own messagebody.
        // Right now, the MessageBody is the complete AS4Message; every OutMessage refers to that same messagebody which 
        // is not correct.
        // At this stage, there should be no AS4-message in my opinion, only UserMessages and SignalMessages.
        private readonly IAS4MessageBodyPersister _as4MessageBodyPersister;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep" /> class.
        /// </summary>
        public StoreAS4MessageStep() : this(Registry.Instance.MessageBodyPersisterProvider) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreAS4MessageStep"/> class.
        /// </summary>
        /// <param name="as4MessageBodyPersister">The as 4 Message Body Persister.</param>
        public StoreAS4MessageStep(IAS4MessageBodyPersister as4MessageBodyPersister)
        {
            _as4MessageBodyPersister = as4MessageBodyPersister;
        }

        /// <summary>
        /// Store the <see cref="AS4Message" /> as OutMessage inside the DataStore
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken token)
        {
            await TryStoreOutMessagesAsync(internalMessage.AS4Message, token).ConfigureAwait(false);

            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task TryStoreOutMessagesAsync(AS4Message as4Message, CancellationToken token)
        {
            try
            {
                await StoreOutMessagesAsync(as4Message, token).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                throw ThrowAS4ExceptionWithInnerException(as4Message, exception);
            }
        }

        private static AS4Exception ThrowAS4ExceptionWithInnerException(AS4Message message, Exception exception)
        {
            return AS4ExceptionBuilder
                .WithDescription("Unable to store AS4 Messages")
                .WithInnerException(exception)
                .WithSendingPMode(message.SendingPMode)
                .WithMessageIds(message.MessageIds)
                .Build();
        }

        private async Task StoreOutMessagesAsync(AS4Message as4Message, CancellationToken token)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var service = new OutMessageService(new DatastoreRepository(context), _as4MessageBodyPersister);

                await service.InsertAS4Message(as4Message, Operation.ToBeSent, token);

                await context.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
    }
}