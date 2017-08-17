using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    [Obsolete("This step is obsolete")]
    public class PullUpdateMessageStatusStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullUpdateMessageStatusStep"/> class.
        /// </summary>
        public PullUpdateMessageStatusStep() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PullUpdateMessageStatusStep" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public PullUpdateMessageStatusStep(Func<DatastoreContext> createContext)
        {
            _createContext = createContext;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            await UpdateMessageStatus(messagingContext.AS4Message);
            return StepResult.Success(messagingContext);
        }

        private async Task UpdateMessageStatus(AS4Message as4Message)
        {
            if (as4Message == null || as4Message.IsSignalMessage)
            {
                return;
            }

            Logger.Info($"[{as4Message.GetPrimaryMessageId()}] Update OutMessage with Status = 'Sent' and Operation = 'Sent'");

            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);

                repository.UpdateOutMessages(
                    as4Message.MessageIds,
                    outMessage =>
                    {
                        outMessage.Operation = Operation.Sent;
                        outMessage.Status = OutStatus.Sent;
                    });


                await context.SaveChangesAsync();
            }
        }
    }
}
