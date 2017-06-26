using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using NLog;
using NonRepudiationInformation = Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the AS4 Receipt must be created
    /// </summary>
    public class CreateAS4ReceiptStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// It is only executed when the external message (received) is an AS4 UserMessage
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            AS4Message receiptMessage = CreateReceiptAS4MessageFor(messagingContext);

            var message = new MessagingContext(receiptMessage, MessagingContextMode.Receive)
            {
                SendingPMode = messagingContext.SendingPMode,
                ReceivingPMode = messagingContext.ReceivingPMode
            };

            return await StepResult.SuccessAsync(message);
        }

        private static AS4Message CreateReceiptAS4MessageFor(MessagingContext messagingContext)
        {
            AS4Message receivedAS4Message = messagingContext.AS4Message;

            // Should we create a Receipt for each and every UserMessage that can be present in the bundle ?
            // If no UserMessages are present, an Empty AS4Message should be returned.
            AS4Message receiptMessage = AS4Message.Create(messagingContext.SendingPMode);

            foreach (string messageId in receivedAS4Message.UserMessages.Select(m => m.MessageId))
            {
                var receipt = new Receipt { RefToMessageId = messageId };
                AdaptReceiptMessage(receipt, messagingContext);

                receiptMessage.SignalMessages.Add(receipt);
            }

            receiptMessage.SigningId = receivedAS4Message.SigningId;

            return receiptMessage;
        }

        private static void AdaptReceiptMessage(Receipt receipt, MessagingContext messagingContext)
        {
            AS4Message receivedAS4Message = messagingContext.AS4Message;
            if (messagingContext.ReceivingPMode?.ReceiptHandling.UseNNRFormat == true)
            {
                Logger.Debug(
                    $"{receivedAS4Message.GetPrimaryMessageId()} Use Non-Repudiation for Receipt {receipt.MessageId} Creation");
                receipt.NonRepudiationInformation = GetNonRepudiationInformationFrom(receivedAS4Message);
            }
            else
            {
                receipt.UserMessage = receivedAS4Message.PrimaryUserMessage;
            }

            // If the Receipt is a Receipt on a MultihopMessage, then we'll need to add some routing-info.
            if (receivedAS4Message.IsMultiHopMessage)
            {
                Logger.Debug("The received UserMessage has been sent via MultiHop.  Send Receipt as MultiHop as well.");

                receipt.MultiHopRouting = AS4Mapper.Map<RoutingInputUserMessage>(receivedAS4Message.PrimaryUserMessage);
            }
        }

        private static NonRepudiationInformation GetNonRepudiationInformationFrom(AS4Message receivedAS4Message)
        {
            ArrayList references = receivedAS4Message.SecurityHeader.GetReferences();

            return new NonRepudiationInformationBuilder()
                .WithSignedReferences(references).Build();
        }
    }
}