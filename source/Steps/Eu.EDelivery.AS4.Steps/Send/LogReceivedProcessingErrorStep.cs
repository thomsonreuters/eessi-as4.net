using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Steps.Send
{
    [Description("This step makes sure that unexpected errors are logged when something went wrong during the send operation or during the processing of the synchronous response.")]
    [Info("Log unexpected errors")]
    public class LogReceivedProcessingErrorStep : IStep
    {
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReceivedProcessingErrorStep"/> class.
        /// </summary>
        public LogReceivedProcessingErrorStep() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReceivedProcessingErrorStep" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public LogReceivedProcessingErrorStep(Func<DatastoreContext> createContext)
        {
            _createContext = createContext;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellation)
        {
            if (messagingContext.ErrorResult == null)
            {
                return StepResult.Success(messagingContext);
            }

            await InsertReferencedInException(messagingContext, cancellation).ConfigureAwait(false);
            return StepResult.Success(messagingContext);
        }

        private async Task InsertReferencedInException(
            MessagingContext messagingContext,
            CancellationToken cancellation)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var exception = new InException(messagingContext.AS4Message?.PrimarySignalMessage?.RefToMessageId, messagingContext.ErrorResult.Description)
                {
                    InsertionTime = DateTimeOffset.Now,
                    ModificationTime = DateTimeOffset.Now
                };

                repository.InsertInException(exception);
                await context.SaveChangesAsync(cancellation).ConfigureAwait(false);
            }
        }
    }
}