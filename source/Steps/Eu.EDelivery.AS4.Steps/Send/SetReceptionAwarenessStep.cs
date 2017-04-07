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
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start configuring Reception Awareness
        /// </summary>
        /// <param name="internalMessage"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _internalMessage = internalMessage;

            string pmodeId = _internalMessage.AS4Message.SendingPMode.Id;

            if (!IsReceptionAwarenessEnabled())
            {
                return await ReturnSameResult(internalMessage, $"Reception Awareness is not enabled in Sending PMode {pmodeId}");
            }

            await InsertReceptionAwarenessAsync();

            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task<StepResult> ReturnSameResult(InternalMessage internalMessage, string description)
        {
            _logger.Info($"{internalMessage.Prefix} {description}");
            return await StepResult.SuccessAsync(internalMessage);
        }

        private bool IsReceptionAwarenessEnabled()
        {
            return _internalMessage.AS4Message.SendingPMode.Reliability.ReceptionAwareness.IsEnabled;
        }

        private async Task InsertReceptionAwarenessAsync()
        {
            _logger.Info($"{_internalMessage.Prefix} Set Reception Awareness");

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
                
                // For every MessageId for which no ReceptionAwareness entity exists, create one.
                var existingIds = existingReceptionAwarenessEntities.Select(r => r.InternalMessageId);
                var missing = _internalMessage.AS4Message.MessageIds.Where(id => existingIds.Contains(id) == false);

                foreach (var messageId in missing)
                {
                    var receptionAwareness = CreateReceptionAwareness(messageId);
                    repository.InsertReceptionAwareness(receptionAwareness);
                }

                await context.SaveChangesAsync();
            }
        }

        private Entities.ReceptionAwareness CreateReceptionAwareness(string messageId)
        {
            SendingProcessingMode pmode = _internalMessage.AS4Message.SendingPMode;

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
