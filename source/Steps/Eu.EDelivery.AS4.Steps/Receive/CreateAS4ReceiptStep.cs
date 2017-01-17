using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the AS4 Receipt must be created
    /// </summary>
    public class CreateAS4ReceiptStep : IStep
    {
        private readonly ILogger _logger;

        private InternalMessage _internalMessage;
        private AS4Message _originalAS4Message;
        private ReceivingProcessingMode _receivePMode;
        private SendingProcessingMode _sendPMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ReceiptStep"/> class
        /// </summary>
        public CreateAS4ReceiptStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// It is only executed when the external message (received) is an AS4 UserMessage
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Throws exception when AS4 Receipt cannot be created</exception>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            InitializeFields(internalMessage);
            TryCreateAS4ReceiptMessage(internalMessage);

            return StepResult.SuccessAsync(this._internalMessage);
        }

        private void InitializeFields(InternalMessage internalMessage)
        {
            this._internalMessage = internalMessage;
            this._originalAS4Message = internalMessage.AS4Message;
            this._receivePMode = this._originalAS4Message.ReceivingPMode;
            this._sendPMode = this._originalAS4Message.SendingPMode;
        }

        private void TryCreateAS4ReceiptMessage(InternalMessage internalMessage)
        {
            try
            {
                AS4Message receiptMessage = CreateReceiptAS4Message();
                AssignPModesToReceiptAS4Message(receiptMessage);
                this._internalMessage = new InternalMessage(receiptMessage);
            }
            catch (AS4Exception exception)
            {
                this._internalMessage = internalMessage;
                throw ThrowCommonAS4Exception(exception);
            }
        }

        private void AssignPModesToReceiptAS4Message(AS4Message receiptMessage)
        {
            receiptMessage.ReceivingPMode = this._receivePMode;
            receiptMessage.SendingPMode = this._sendPMode;
        }

        private AS4Message CreateReceiptAS4Message()
        {
            string messageId = GetMessageId();

            this._logger.Info($"{this._internalMessage.Prefix} Create Receipt Message with Reference to respond");

            var receipt = new Receipt {RefToMessageId = messageId};
            AS4Message receiptMessage = new AS4MessageBuilder()
                .WithUserMessage(this._originalAS4Message.PrimaryUserMessage).WithSignalMessage(receipt).Build();

            receiptMessage.SigningId = this._originalAS4Message.SigningId;
            AdaptReceiptMessage(receipt);

            return receiptMessage;
        }

        private string GetMessageId()
        {
            return this._originalAS4Message.PrimaryUserMessage != null
                ? this._originalAS4Message.PrimaryUserMessage.MessageId
                : this._originalAS4Message.PrimarySignalMessage.MessageId;
        }

        private void AdaptReceiptMessage(Receipt receipt)
        {
            if (this._receivePMode?.ReceiptHandling.UseNNRFormat == true)
            {
                this._logger.Debug(
                    $"{this._internalMessage.Prefix} Use Non-Repudiation for Receipt {receipt.MessageId} Creation");
                receipt.NonRepudiationInformation = CreateNonRepudiationInformation();
            }
            else receipt.UserMessage = this._originalAS4Message.PrimaryUserMessage;
        }

        private NonRepudiationInformation CreateNonRepudiationInformation()
        {
            ArrayList references = this._originalAS4Message.SecurityHeader.GetReferences();

            return new NonRepudiationInformationBuilder()
                .WithSignedReferences(references).Build();
        }

        private AS4Exception ThrowCommonAS4Exception(AS4Exception exception)
        {
            return new AS4ExceptionBuilder()
                .WithExistingAS4Exception(exception)
                .WithPModeString(this._internalMessage.ReceivingPModeString)
                .WithMessageIds(this._internalMessage.AS4Message.MessageIds)
                .WithReceivingPMode(this._internalMessage.AS4Message.ReceivingPMode)
                .Build();
        }
    }
}