using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the Messages.Out and Messages.In data stores get updated
    /// </summary>
    public class SendUpdateDataStoreStep : IStep
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private AS4Message _as4Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep" /> class
        /// </summary>
        public SendUpdateDataStoreStep() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendUpdateDataStoreStep"/> class.
        /// </summary>
        /// <param name="createDatastoreContext">The create Datastore Context.</param>
        public SendUpdateDataStoreStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
        }

        /// <summary>
        /// Execute the Update DataStore Step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _as4Message = internalMessage.AS4Message;

            using (DatastoreContext context = _createDatastoreContext())
            {
                var inMessageService = new InMessageService(new DatastoreRepository(context));

                foreach (SignalMessage signalMessage in internalMessage.AS4Message.SignalMessages)
                {
                    _logger.Info($"{internalMessage.Prefix} Update SignalMessage {signalMessage.MessageId}");

                    TryUpdateSignalMessage(signalMessage, inMessageService, cancellationToken);
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            return await StepResult.SuccessAsync(internalMessage);
        }

        private void TryUpdateSignalMessage(SignalMessage signalMessage, InMessageService inMessageService, CancellationToken cancellationToken)
        {
            try
            {
                UpdateSignalMessage(signalMessage, inMessageService, cancellationToken);
            }
            catch (Exception exception)
            {
                string description = $"Unable to update SignalMessage {signalMessage.MessageId}";
                throw ThrowAS4UpdateDatastoreException(description, exception);
            }
        }

        private void UpdateSignalMessage(SignalMessage signalMessage, InMessageService inMessageService, CancellationToken cancellationToken)
        {
            if (signalMessage is Receipt)
            {
                UpdateReceipt(signalMessage, inMessageService, cancellationToken);
            }
            else if (signalMessage is Error)
            {
                UpdateError(signalMessage, inMessageService, cancellationToken);
            }
            else
            {
                UpdateOther(signalMessage, inMessageService, cancellationToken);
            }
        }

        private void UpdateReceipt(SignalMessage signalMessage, InMessageService inMessageService, CancellationToken cancellationToken)
        {
            var receipt = signalMessage as Receipt;

            if (receipt != null && receipt.NonRepudiationInformation == null)
            {
                receipt.NonRepudiationInformation = CreateNonRepudiationInformation();
            }

            inMessageService.InsertReceipt(signalMessage, _as4Message, cancellationToken);

            OutStatus status = IsSignalMessageReferenceUserMessage(signalMessage) ? OutStatus.Ack : OutStatus.NotApplicable;

            inMessageService.UpdateSignalMessage(signalMessage, status, cancellationToken);
        }

        private NonRepudiationInformation CreateNonRepudiationInformation()
        {
            ArrayList references = _as4Message.SecurityHeader.GetReferences();

            return new NonRepudiationInformationBuilder().WithSignedReferences(references).Build();
        }

        private void UpdateError(SignalMessage signalMessage, InMessageService inMessageService, CancellationToken cancellationToken)
        {
            inMessageService.InsertError(signalMessage, _as4Message, cancellationToken);

            OutStatus status = IsSignalMessageReferenceUserMessage(signalMessage) ? OutStatus.Nack : OutStatus.NotApplicable;

            inMessageService.UpdateSignalMessage(signalMessage, status, cancellationToken);
        }

        private bool IsSignalMessageReferenceUserMessage(SignalMessage signalMessage)
        {
            return signalMessage.RefToMessageId?.Equals(_as4Message.PrimaryUserMessage?.MessageId) ?? false;
        }

        private static void UpdateOther(SignalMessage signalMessage, InMessageService inMessageService, CancellationToken cancellationToken)
        {
            inMessageService.UpdateSignalMessage(signalMessage, OutStatus.Sent, cancellationToken);
        }

        private AS4Exception ThrowAS4UpdateDatastoreException(string description, Exception innerException)
        {
            _logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithInnerException(innerException)
                .WithMessageIds(_as4Message.MessageIds)
                .WithSendingPMode(_as4Message.SendingPMode)
                .Build();
        }
    }
}