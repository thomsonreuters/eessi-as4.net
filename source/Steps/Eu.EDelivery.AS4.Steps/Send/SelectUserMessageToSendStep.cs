using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how a MessageUnit should be selected to be sent via Pulling.
    /// </summary>
    /// <seealso cref="IStep" />
    public class SelectUserMessageToSendStep : IStep
    {
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectUserMessageToSendStep" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public SelectUserMessageToSendStep(Func<DatastoreContext> createContext)
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
            throw new System.NotImplementedException();
        }
    }
}
