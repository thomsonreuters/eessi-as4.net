using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;
using Receipt = Eu.EDelivery.AS4.Model.Core.Receipt;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how the AS4 Receipt must be created
    /// </summary>
    [Info("Create a Receipt message")]
    [Description("Create an AS4 Receipt message to inform the sender that the received AS4 Message has been processed correctly")]
    public class CreateAS4ReceiptStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// It is only executed when the external message (received) is an AS4 UserMessage
        /// </summary>
        /// <param name="messagingContext"></param>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            AS4Message receivedAS4Message = messagingContext.AS4Message;
            if (receivedAS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateAS4ReceiptStep)} requires an AS4Message to create a Receipt but no AS4Message is present in the MessagingContext");
            }

            AS4Message receiptMessage = AS4Message.Empty;
            receiptMessage.SigningId = receivedAS4Message.SigningId;

            foreach (UserMessage userMessage in receivedAS4Message.UserMessages)
            {
                Receipt receipt = CreateReferencedReceipt(userMessage, receivedAS4Message, messagingContext.ReceivingPMode);
                receiptMessage.AddMessageUnit(receipt);
            }

            if (Logger.IsInfoEnabled && receiptMessage.MessageUnits.Any())
            {
                Logger.Info(
                    $"{messagingContext.LogTag} {receiptMessage.MessageUnits.Count()} Receipt message(s) has been created for received AS4 UserMessages");
            }

            messagingContext.ModifyContext(receiptMessage);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private static Receipt CreateReferencedReceipt(
            UserMessage userMessage,
            AS4Message received,
            ReceivingProcessingMode receivingPMode)
        {
            bool useNRRFormat = receivingPMode?.ReplyHandling.ReceiptHandling.UseNRRFormat ?? false;
            if (useNRRFormat && !received.IsSigned)
            {
                Logger.Warn(
                    $"ReceivingPMode {receivingPMode?.Id} is configured to reply with Non-Repudation Receipts, " + 
                    $"but incoming UserMessage {userMessage.MessageId} isn\'t signed");
            }
            else if (!useNRRFormat)
            {
                Logger.Debug(
                    $"ReceivingPMode {receivingPMode?.Id} is configured to not use the Non-Repudiation format." + 
                    $"This means the original UserMessage {userMessage.MessageId} will be included in the Receipt");
            }

            if (received.IsMultiHopMessage)
            {
                Logger.Debug($"Because the received UserMessage {userMessage.MessageId} has been sent via MultiHop, the Receipt will be send as MultiHop also");
            }

            if (useNRRFormat && received.IsSigned)
            {
                    Logger.Debug($"ReceivingPMode {receivingPMode?.Id} is configured to use Non-Repudiation for Receipt Creation");
                    return Receipt.CreateReferencingNonRepudiation(
                        IdentifierFactory.Instance.Create(), 
                        userMessage,
                        received.SecurityHeader, 
                        received.IsMultiHopMessage);
            }

            return Receipt.CreateReferencing(
                IdentifierFactory.Instance.Create(), 
                userMessage, 
                received.IsMultiHopMessage);
        }
    }
}