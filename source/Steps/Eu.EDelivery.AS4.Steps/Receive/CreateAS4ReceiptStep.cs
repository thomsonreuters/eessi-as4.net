using System.Collections;
using System.Linq;
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
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// It is only executed when the external message (received) is an AS4 UserMessage
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Throws exception when AS4 Receipt cannot be created</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            InitializeFields(internalMessage);
            TryCreateAS4ReceiptMessage(internalMessage);

            return await StepResult.SuccessAsync(_internalMessage);
        }

        private void InitializeFields(InternalMessage internalMessage)
        {
            _internalMessage = internalMessage;
            _originalAS4Message = internalMessage.AS4Message;
            _receivePMode = _originalAS4Message.ReceivingPMode;
            _sendPMode = _originalAS4Message.SendingPMode;
        }

        private void TryCreateAS4ReceiptMessage(InternalMessage internalMessage)
        {
            try
            {
                AS4Message receiptMessage = CreateReceiptAS4Message();

                AssignPModesToReceiptAS4Message(receiptMessage);
                _internalMessage = new InternalMessage(receiptMessage);
            }
            catch (AS4Exception exception)
            {
                _internalMessage = internalMessage;
                throw ThrowCommonAS4Exception(exception);
            }
        }

        private void AssignPModesToReceiptAS4Message(AS4Message receiptMessage)
        {
            receiptMessage.ReceivingPMode = _receivePMode;
            receiptMessage.SendingPMode = _sendPMode;
        }

        private AS4Message CreateReceiptAS4Message()
        {
            // Should we create a Receipt for each and every UserMessage that can be present in the bundle ?
            // If no UserMessages are present, an Empty AS4Message should be returned.
            AS4MessageBuilder messageBuilder = new AS4MessageBuilder();

            foreach (var messageId in _originalAS4Message.UserMessages.Select(m => m.MessageId))
            {
                var receipt = new Receipt { RefToMessageId = messageId };
                AdaptReceiptMessage(receipt);

                messageBuilder.WithSignalMessage(receipt);
            }

            var receiptMessage = messageBuilder.Build();

            receiptMessage.SigningId = _originalAS4Message.SigningId;

            return receiptMessage;
        }

        private void AdaptReceiptMessage(Receipt receipt)
        {
            if (_receivePMode?.ReceiptHandling.UseNNRFormat == true)
            {
                _logger.Debug(
                    $"{_internalMessage.Prefix} Use Non-Repudiation for Receipt {receipt.MessageId} Creation");
                receipt.NonRepudiationInformation = CreateNonRepudiationInformation();
            }
            else
            {
                receipt.UserMessage = _originalAS4Message.PrimaryUserMessage;
            }
            
            // If the receipt should not contain NonRepudiationInformation, or the 
            // Receipt is a Receipt on a MultihopMessage, then we'll need the original
            // UserMessage.
            if (_sendPMode.MessagePackaging.IsMultiHop)
            {
                receipt.RelatedUserMessageForMultihop = _originalAS4Message.PrimaryUserMessage;
            }
        }

        private NonRepudiationInformation CreateNonRepudiationInformation()
        {
            ArrayList references = _originalAS4Message.SecurityHeader.GetReferences();

            return new NonRepudiationInformationBuilder()
                .WithSignedReferences(references).Build();
        }

        private AS4Exception ThrowCommonAS4Exception(AS4Exception exception)
        {
            return AS4ExceptionBuilder
                .WithDescription("An error occured while receiving a message.")
                .WithExistingAS4Exception(exception)
                .WithPModeString(_internalMessage.ReceivingPModeString)
                .WithMessageIds(_internalMessage.AS4Message.MessageIds)
                .WithReceivingPMode(_internalMessage.AS4Message.ReceivingPMode)
                .Build();
        }
    }
}