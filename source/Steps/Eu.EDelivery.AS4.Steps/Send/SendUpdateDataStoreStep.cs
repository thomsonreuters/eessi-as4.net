using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the Messages.Out and Messages.In data stores get updated
    /// </summary>
    public class SendUpdateDataStoreStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IInMessageService _inMessageService;
        private AS4Message _as4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep"/> class
        /// </summary>
        public SendUpdateDataStoreStep()
        {
            this._inMessageService = new InMessageService(Registry.Instance.DatastoreRepository);
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep"/> class. 
        /// Create an Update Data Store step
        /// with a given Repository
        /// </summary>
        /// <param name="inMessageService">
        /// </param>
        public SendUpdateDataStoreStep(IInMessageService inMessageService)
        {
            this._inMessageService = inMessageService;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Execute the Update DataStore Step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._as4Message = internalMessage.AS4Message;

            foreach (SignalMessage signalMessage in internalMessage.AS4Message.SignalMessages)
            {
                this._logger.Info($"{internalMessage.Prefix} Update SignalMessage {signalMessage.MessageId}");
                await TryUpdateSignalMessage(signalMessage, cancellationToken);
            }

            return StepResult.Success(internalMessage);
        }

        private async Task TryUpdateSignalMessage(SignalMessage signalMessage, CancellationToken cancellationToken)
        {
            try
            {
                await UpdateSignalMessage(signalMessage, cancellationToken);
            }
            catch (Exception exception)
            {
                string description = $"Unable to update SignalMessage {signalMessage.MessageId}";
                throw ThrowAS4UpdateDatastoreException(description, exception);
            }
        }

        private async Task UpdateSignalMessage(SignalMessage signalMessage, CancellationToken cancellationToken)
        {
            if (signalMessage is Receipt) await UpdateReceipt(signalMessage, cancellationToken);
            else if (signalMessage is Error) await UpdateError(signalMessage, cancellationToken);
            else await UpdateOther(signalMessage, cancellationToken);
        }

        private async Task UpdateReceipt(SignalMessage signalMessage, CancellationToken cancellationToken)
        {
            await this._inMessageService.InsertReceiptAsync(signalMessage, this._as4Message, cancellationToken);

            OutStatus status = IsSignalMessageReferenceUserMessage(signalMessage)
                ? OutStatus.Ack
                : OutStatus.NotApplicable;

            await this._inMessageService.UpdateSignalMessage(signalMessage, status, cancellationToken);
        }

        private async Task UpdateError(SignalMessage signalMessage, CancellationToken cancellationToken)
        {
            await this._inMessageService.InsertErrorAsync(signalMessage, this._as4Message, cancellationToken);

            OutStatus status = IsSignalMessageReferenceUserMessage(signalMessage)
                ? OutStatus.Nack
                : OutStatus.NotApplicable;

            await this._inMessageService.UpdateSignalMessage(signalMessage, status, cancellationToken);
        }

        private bool IsSignalMessageReferenceUserMessage(SignalMessage signalMessage)
        {
            return signalMessage.RefToMessageId.Equals(this._as4Message.PrimaryUserMessage?.MessageId);
        }

        private async Task UpdateOther(SignalMessage signalMessage, CancellationToken cancellationToken)
        {
            await this._inMessageService.UpdateSignalMessage(signalMessage, OutStatus.Sent, cancellationToken);
        }

        private AS4Exception ThrowAS4UpdateDatastoreException(string description, Exception innerException)
        {
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(this._as4Message.MessageIds)
                .WithSendingPMode(this._as4Message.SendingPMode)
                .Build();
        }
    }
}