using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the state and configuration on the retry mechanism of reception awareness is stored
    /// </summary>
    public class SetReceptionAwarenessStep : IStep
    {
        private readonly ILogger _logger;

        private InternalMessage _internalMessage;


        /// <summary>
        /// Initializes a new instance of the <see cref="SetReceptionAwarenessStep"/> class. 
        /// Create a <see cref="IStep"/> implementation
        /// which is responsible for the retry mechanism of the reception awareness</summary>        
        public SetReceptionAwarenessStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start configuring Reception Awareness
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;

            string pmodeId = this._internalMessage.AS4Message.SendingPMode.Id;

            if (!IsReceptionAwarenessEnabled())
                return await ReturnSameResult(internalMessage, $"Reception Awareness is not enabled in Sending PMode {pmodeId}");

            await InsertReceptionAwarenessAsync();

            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task<StepResult> ReturnSameResult(InternalMessage internalMessage, string description)
        {
            this._logger.Info($"{internalMessage.Prefix} {description}");
            return await StepResult.SuccessAsync(internalMessage);
        }

        private bool IsReceptionAwarenessEnabled()
        {
            return this._internalMessage.AS4Message.SendingPMode
                .Reliability.ReceptionAwareness.IsEnabled;
        }

        private async Task InsertReceptionAwarenessAsync()
        {
            this._logger.Info($"{this._internalMessage.Prefix} Set Reception Awareness");

            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                var existingReceptionAwarenessEntities =
                    repository.GetReceptionAwareness(_internalMessage.AS4Message.MessageIds).ToArray();

                // Update existing entities and create new one if we do not have one already
                foreach (var e in existingReceptionAwarenessEntities)
                {
                    e.CurrentRetryCount += 1;
                    e.LastSendTime = DateTimeOffset.UtcNow;
                }

                // TODO: optimize; SaveChanges should be called at the end of the transaction.
                //       Now, the repository calls SaveChanges as well, which should not be 
                //       done imho.
                context.SaveChanges();

                // For every MessageId for which no ReceptionAwareness entity exists, create one.
                var existingIds = existingReceptionAwarenessEntities.Select(r => r.InternalMessageId);
                var missing = _internalMessage.AS4Message.MessageIds.Where(id => existingIds.Contains(id) == false);

                foreach (var messageId in missing)
                {
                    var receptionAwareness = CreateReceptionAwareness(messageId);
                    await repository.InsertReceptionAwarenessAsync(receptionAwareness);
                }
            }
        }

        private Entities.ReceptionAwareness CreateReceptionAwareness(string messageId)
        {
            SendingProcessingMode pmode = this._internalMessage.AS4Message.SendingPMode;

            return new Entities.ReceptionAwareness
            {
                InternalMessageId = messageId,
                CurrentRetryCount = 0,
                TotalRetryCount = pmode.Reliability.ReceptionAwareness.RetryCount,
                RetryInterval = pmode.Reliability.ReceptionAwareness.RetryInterval,
                LastSendTime = DateTimeOffset.UtcNow,
                IsCompleted = false
            };
        }
    }
}
