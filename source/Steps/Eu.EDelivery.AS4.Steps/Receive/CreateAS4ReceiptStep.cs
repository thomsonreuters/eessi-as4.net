using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using NLog;
using NonRepudiationInformation = Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the AS4 Receipt must be created
    /// </summary>
    [Description("Create an AS4 Receipt message to inform the sender that the received AS4 Message has been processed correctly")]
    [Info("Create a Receipt message")]
    public class CreateAS4ReceiptStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// It is only executed when the external message (received) is an AS4 UserMessage
        /// </summary>
        /// <param name="messagingContext"></param>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            AS4Message receivedAS4Message = messagingContext.AS4Message;

            // Should we create a Receipt for each and every UserMessage that can be present in the bundle ?
            // If no UserMessages are present, an Empty AS4Message should be returned.
            AS4Message receiptMessage = AS4Message.Create(messagingContext.SendingPMode);
            receiptMessage.SigningId = receivedAS4Message.SigningId;

            foreach (string messageId in receivedAS4Message.UserMessages.Select(m => m.MessageId))
            {
                Receipt receipt = CreateReferencedReceipt(messageId, messagingContext);
                receiptMessage.AddMessageUnit(receipt);
            }

            if (Logger.IsInfoEnabled && receiptMessage.MessageUnits.Any())
            {
                Logger.Info($"{messagingContext.LogTag} Receipt message has been created for received AS4 UserMessages.");
            }

            messagingContext.ModifyContext(receiptMessage);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private static Receipt CreateReferencedReceipt(string ebmsMessageId, MessagingContext messagingContext)
        {
            var receipt = new Receipt { RefToMessageId = ebmsMessageId };
            AS4Message receivedAS4Message = messagingContext.AS4Message;
            bool useNRRFormat = messagingContext.ReceivingPMode?.ReplyHandling.ReceiptHandling.UseNRRFormat ?? false;

            if (useNRRFormat)
            {
                if (receivedAS4Message.IsSigned)
                {
                    Logger.Debug(
                        $"{messagingContext.LogTag} Receiving PMode {messagingContext.ReceivingPMode?.Id} " + 
                        $"is configured to use Non-Repudiation for Receipt {receipt.MessageId} Creation");

                    receipt.NonRepudiationInformation = GetNonRepudiationInformationFrom(receivedAS4Message);
                }
                else
                {
                    Logger.Warn(
                        $"{messagingContext.LogTag} Receiving PMode ({messagingContext.ReceivingPMode?.Id}) " + 
                        "is configured to reply with Non-Repudation Receipts, but incoming UserMessage isn't signed");

                    receipt.UserMessage = receivedAS4Message.FirstUserMessage;
                }
            }
            else
            {
                Logger.Debug(
                    $"{messagingContext.LogTag} Receiving PMode is configured to not use the Non-Repudiation format." + 
                    "This means the original UserMessage will be included in the Receipt");
                receipt.UserMessage = receivedAS4Message.FirstUserMessage;
            }

            // If the Receipt is a Receipt on a MultihopMessage, then we'll need to add some routing-info.
            if (receivedAS4Message.IsMultiHopMessage)
            {
                Logger.Debug(
                    $"{messagingContext.LogTag} Because the received UserMessage has been sent via MultiHop, " + 
                    "we will send the Receipt as MultiHop also");

                receipt.MultiHopRouting = 
                    AS4Mapper.Map<RoutingInputUserMessage>(receivedAS4Message.FirstUserMessage);
            }

            return receipt;
        }

        private static NonRepudiationInformation GetNonRepudiationInformationFrom(AS4Message receivedAS4Message)
        {
            IEnumerable<Reference> references = 
                receivedAS4Message.SecurityHeader.GetReferences();

            return new NonRepudiationInformationBuilder()
                .WithSignedReferences(references)
                .Build();
        }
    }
}