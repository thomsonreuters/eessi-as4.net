using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    /// <summary>
    /// Transform the given <see cref="ReceptionAwareness"/> Model
    /// to a <see cref="InternalMessage"/> Model
    /// </summary>
    public class ReceptionAwarenessTransformer : ITransformer
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Create a new <see cref="ITransformer"/> implementation
        /// to transform to <see cref="ReceptionAwareness"/>
        /// </summary>
        public ReceptionAwarenessTransformer()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Transform the given Message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            ReceivedEntityMessage entityMessage = RetrieveEntityMessage(message);
            ReceptionAwareness awareness = RetrieveReceptionAwareness(entityMessage);
            var internalMessage = new InternalMessage {ReceiptionAwareness = awareness};

            this._logger.Info($"[{awareness.InternalMessageId}] Receiption Awareness is successfully transformed");
            return Task.FromResult(internalMessage);
        }

        private ReceptionAwareness RetrieveReceptionAwareness(ReceivedEntityMessage messageEntity)
        {
            var receptionAwareness = messageEntity.Entity as ReceptionAwareness;
            if (receptionAwareness == null) throw ThrowNotSupportedAS4Exception();

            return receptionAwareness;
        }

        private ReceivedEntityMessage RetrieveEntityMessage(ReceivedMessage message)
        {
            var entityMessage = message as ReceivedEntityMessage;
            if (entityMessage == null) throw ThrowNotSupportedAS4Exception();

            return entityMessage;
        }

        private AS4Exception ThrowNotSupportedAS4Exception()
        {
            const string description =
                "Current Transformer cannot be used for the given Received Message, expecting type of ReceivedEntityMessage";
            this._logger.Error(description);
            
            return new AS4ExceptionBuilder().WithDescription(description).Build();
        }
    }
}