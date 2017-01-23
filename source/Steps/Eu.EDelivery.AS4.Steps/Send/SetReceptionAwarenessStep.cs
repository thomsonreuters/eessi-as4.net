using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly IDatastoreRepository _repository;

        private InternalMessage _internalMessage;
        private ICollection<string> _messageIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetReceptionAwarenessStep"/> class. 
        /// Create a <see cref="IStep"/> implementation
        /// which is responsible for the retry mechanism of the reception awareness
        /// </summary>
        /// <param name="repository">
        /// </param>
        public SetReceptionAwarenessStep(IDatastoreRepository repository)
        {
            this._repository = repository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public SetReceptionAwarenessStep()
        {
            this._repository = Registry.Instance.DatastoreRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start configuring Reception Awareness
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;
            this._messageIds = new Collection<string>();
            string pmodeId = this._internalMessage.AS4Message.SendingPMode.Id;

            if (!IsReceptionAwarenessEnabled())
                return ReturnSameResult(internalMessage, $"Reception Awareness is not enabled in Sending PMode {pmodeId}");

            if (!IsReceptionAwarenessNotSet())
                return ReturnSameResult(internalMessage, "ebMS message already is configured with Reception Awareness");

            await InsertReceptionAwarenessAsync();
            return StepResult.Success(internalMessage);
        }

        private StepResult ReturnSameResult(InternalMessage internalMessage, string description)
        {
            this._logger.Info($"{internalMessage.Prefix} {description}");
            return StepResult.Success(internalMessage);
        }

        private bool IsReceptionAwarenessEnabled()
        {
            return this._internalMessage.AS4Message.SendingPMode
                .Reliability.ReceptionAwareness.IsEnabled;
        }

        private bool IsReceptionAwarenessNotSet()
        {
            string[] messageIds = this._internalMessage.AS4Message.MessageIds;

            for (int i = 0, l = messageIds.Length; i < l; i++)
                if (DoesDatastoreContainsReceptionAwareness(i))
                    this._messageIds.Add(messageIds[i]);

            return this._messageIds.Count == 0;
        }

        private bool DoesDatastoreContainsReceptionAwareness(int index)
        {
            string[] messageIds = this._internalMessage.AS4Message.MessageIds;
            return this._repository.GetReceptionAwareness(messageIds[index]) != null;
        }

        private async Task InsertReceptionAwarenessAsync()
        {
            this._logger.Info($"{this._internalMessage.Prefix} Set Reception Awareness");
            string[] messageIds = this._internalMessage.AS4Message.MessageIds;

            foreach(string messageId in messageIds)
            {
                Entities.ReceptionAwareness receptionAwareness = CreateReceptionAwareness(messageId);
                await this._repository.InsertReceptionAwarenessAsync(receptionAwareness);
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
