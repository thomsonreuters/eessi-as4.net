using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class DetermineMessageHandlingStep : IStep
    {
        private readonly Func<DatastoreContext> _createDatastoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetermineMessageHandlingStep"/> class.
        /// </summary>
        public DetermineMessageHandlingStep() : this(Registry.Instance.CreateDatastoreContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DetermineMessageHandlingStep"/> class.
        /// </summary>
        public DetermineMessageHandlingStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {            
            return StepResult.Success(messagingContext);          
        }
    }
}
