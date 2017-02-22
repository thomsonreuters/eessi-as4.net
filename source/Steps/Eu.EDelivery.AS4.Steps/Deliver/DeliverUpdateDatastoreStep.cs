using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is delivered
    /// </summary>
    public class DeliverUpdateDatastoreStep : IStep
    {        
        private readonly ILogger _logger;

        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliverUpdateDatastoreStep"/> class
        /// </summary>
        public DeliverUpdateDatastoreStep()
        {
     
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the InMessages
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;
            this._logger.Info($"{this._internalMessage.Prefix} Update AS4 UserMessages in Datastore");

            await UpdateUserMessageAsync(internalMessage.DeliverMessage);
            return StepResult.Success(internalMessage);
        }

        private async Task UpdateUserMessageAsync(DeliverMessageEnvelope deliverMessage)
        {
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                string messageId = deliverMessage.MessageInfo.MessageId;
                this._logger.Info($"{this._internalMessage.Prefix} Update InMessage with Delivered Status and Operation");

                await repository.UpdateInMessageAsync(messageId, UpdateNotifiedInMessage);
            }
        }

        private static void UpdateNotifiedInMessage(InMessage inMessage)
        {
            inMessage.Status = InStatus.Delivered;
            inMessage.Operation = Operation.Delivered;
        }
    }
}
