using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;

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
            if (messagingContext.AS4Message.IsSignalMessage && messagingContext.AS4Message.IsMultiHopMessage == false)
            {
                return StepResult.Success(messagingContext);
            }

            if (messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException("The Receiving PMode for this message has not been determined yet.");
            }

            if (messagingContext.ReceivingPMode.MessageHandling.MessageHandlingType == MessageHandlingChoiceType.Forward)
            {
                using (var db = _createDatastoreContext())
                {
                    var repository = new DatastoreRepository(db);
                    repository.UpdateInMessage(messagingContext.EbmsMessageId, message => message.Operation = Operation.ToBeForwarded);

                    await db.SaveChangesAsync(cancellationToken);
                }

                // When the Message has to be forwarded, the remaining Steps must not be executed.
                // The MSH must answer with a HTTP Accepted status-code, so an empty context must be returned.
                messagingContext.ModifyContext(AS4Message.Empty);

                return StepResult.Success(messagingContext).AndStopExecution();
            }

            // When the message must be delivered, no further action needs to be done in this step.
            return StepResult.Success(messagingContext);

        }
    }
}
