using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Transform the given <see cref="ReceptionAwareness" /> Model
    /// to a <see cref="MessagingContext" /> Model
    /// </summary>
    public class ReceptionAwarenessTransformer : ITransformer
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform the given Message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            ReceivedEntityMessage entityMessage = RetrieveEntityMessage(message);
            ReceptionAwareness awareness = RetrieveReceptionAwareness(entityMessage);
            var internalMessage = new MessagingContext(awareness);

            Logger.Info($"[{awareness.InternalMessageId}] Reception Awareness is successfully transformed");
            return await Task.FromResult(internalMessage);
        }

        private static ReceptionAwareness RetrieveReceptionAwareness(ReceivedEntityMessage messageEntity)
        {
            var receptionAwareness = messageEntity.Entity as ReceptionAwareness;
            if (receptionAwareness == null)
            {
                throw new NotSupportedException($"Reception Awareness Transformer only supports '{nameof(ReceptionAwareness)}'");
            }

            return receptionAwareness;
        }

        private static ReceivedEntityMessage RetrieveEntityMessage(ReceivedMessage message)
        {
            var entityMessage = message as ReceivedEntityMessage;
            if (entityMessage == null)
            {
                throw new NotSupportedException($"Reception Awareness Transformer only supports '{nameof(ReceivedEntityMessage)}'");
            }

            return entityMessage;
        }
    }
}