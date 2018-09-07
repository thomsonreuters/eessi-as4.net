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
    [Info("Log unexpected errors")]
    [Description(
        "This step makes sure that unexpected errors are logged when something went wrong " + 
        "during the send operation or during the processing of the synchronous response.")]
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
            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

            _createContext = createContext;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext?.ErrorResult == null)
            {
                return StepResult.Success(messagingContext);
            }

            await InsertReferencedInException(messagingContext, CancellationToken.None).ConfigureAwait(false);
            return StepResult.Success(messagingContext);
        }

        private async Task InsertReferencedInException(
            MessagingContext messagingContext,
            CancellationToken cancellation)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var exception = InException.ForEbmsMessageId(
                    messagingContext.AS4Message?.FirstSignalMessage?.RefToMessageId, 
                    new Exception(messagingContext.ErrorResult.Description));

                repository.InsertInException(exception);
                await context.SaveChangesAsync(cancellation).ConfigureAwait(false);
            }
        }
    }
}