using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start configuring Reception Awareness
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (!IsReceptionAwarenessEnabled(internalMessage))
            {
                string pmodeId = internalMessage.SendingPMode.Id;
                return await ReturnSameResult(internalMessage, $"Reception Awareness is not enabled in Sending PMode {pmodeId}");
            }

            await InsertReceptionAwarenessAsync(internalMessage).ConfigureAwait(false);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private static bool IsReceptionAwarenessEnabled(InternalMessage internalMessage)
        {
            return internalMessage.SendingPMode.Reliability.ReceptionAwareness.IsEnabled;
        }

        private static async Task<StepResult> ReturnSameResult(InternalMessage internalMessage, string description)
        {
            Logger.Info($"{internalMessage.Prefix} {description}");
            return await StepResult.SuccessAsync(internalMessage);
        }

        private static async Task InsertReceptionAwarenessAsync(InternalMessage internalMessage)
        {
            Logger.Info($"{internalMessage.Prefix} Set Reception Awareness");
            AS4Message as4Message = internalMessage.AS4Message;

            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                Entities.ReceptionAwareness[] existingReceptionAwarenessEntities =
                    repository.GetReceptionAwareness(as4Message.MessageIds).ToArray();

                // Update existing entities and create new one if we do not have one already
                UpdateCurrentRetryCountFor(existingReceptionAwarenessEntities);

                // For every MessageId for which no ReceptionAwareness entity exists, create one.
                InsertReceptionAwarenessForMessagesWithout(internalMessage, repository, existingReceptionAwarenessEntities);

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static void UpdateCurrentRetryCountFor(IEnumerable<Entities.ReceptionAwareness> existingReceptionAwarenessEntities)
        {
            foreach (Entities.ReceptionAwareness awareness in existingReceptionAwarenessEntities)
            {
                awareness.CurrentRetryCount += 1;
                awareness.LastSendTime = DateTimeOffset.UtcNow;
            }
        }

        private static void InsertReceptionAwarenessForMessagesWithout(
            InternalMessage message,
            IDatastoreRepository repository,
            IEnumerable<Entities.ReceptionAwareness> existingReceptionAwarenessEntities)
        {
            AS4Message as4Message = message.AS4Message;
            IEnumerable<string> existingIds = existingReceptionAwarenessEntities.Select(r => r.InternalMessageId);
            IEnumerable<string> messageIdsWithoutReceptionAwareness = as4Message.MessageIds.Where(id => existingIds.Contains(id) == false);

            foreach (string messageId in messageIdsWithoutReceptionAwareness)
            {
                Entities.ReceptionAwareness receptionAwareness = CreateReceptionAwareness(messageId, message.SendingPMode);
                repository.InsertReceptionAwareness(receptionAwareness);
            }
        }

        private static Entities.ReceptionAwareness CreateReceptionAwareness(string messageId, SendingProcessingMode pmode)
        {
            return new Entities.ReceptionAwareness
            {
                InternalMessageId = messageId,
                CurrentRetryCount = 0,
                TotalRetryCount = pmode.Reliability.ReceptionAwareness.RetryCount,
                RetryInterval = pmode.Reliability.ReceptionAwareness.RetryInterval,
                LastSendTime = DateTimeOffset.UtcNow,
                Status = ReceptionStatus.Pending
            };
        }
    }
}