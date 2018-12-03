using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

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

            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateAS4ReceiptStep)} requires an AS4Message to create ebMS Receipts but no AS4Message is present in the MessagingContext");
            }

            bool receiptSigning = messagingContext.ReceivingPMode?.ReplyHandling?.ResponseSigning?.IsEnabled ?? false;
            bool useNRRFormat = messagingContext.ReceivingPMode?.ReplyHandling?.ReceiptHandling?.UseNRRFormat ?? false;

            if (!receiptSigning && useNRRFormat)
            {
                Logger.Error(
                    "Cannot create Non-Repudiation Receipts that aren\'t signed, please change either the "
                    + $"ReceivingPMode {messagingContext.ReceivingPMode.Id} ReplyHandling.ReceiptHandling.UseNRRFormat or the ReplyHandling.ResponseSigning");

                messagingContext.ErrorResult = new ErrorResult(
                    "Cannot create Non-Repudiation Receipts that aren't signed",
                    ErrorAlias.InvalidReceipt);

                return StepResult.Failed(messagingContext);
            }

            AS4Message receivedMessage = messagingContext.AS4Message;
            AS4Message receiptMessage = AS4Message.Empty;
            receiptMessage.SigningId = receivedMessage.SigningId;

            foreach (UserMessage userMessage in receivedMessage.UserMessages)
            {
                Receipt receipt = CreateReferencedReceipt(userMessage, receivedMessage, messagingContext.ReceivingPMode);
                receiptMessage.AddMessageUnit(receipt);
            }

            if (Logger.IsInfoEnabled && receiptMessage.MessageUnits.Any())
            {
                Logger.Info($"{messagingContext.LogTag} {receiptMessage.MessageUnits.Count()} Receipt message(s) has been created for received AS4 UserMessages");
            }

            messagingContext.ModifyContext(receiptMessage);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private static Receipt CreateReferencedReceipt(
            UserMessage userMessage,
            AS4Message received,
            ReceivingProcessingMode receivingPMode)
        {
            bool useNRRFormat = receivingPMode?.ReplyHandling?.ReceiptHandling?.UseNRRFormat ?? false;
            if (useNRRFormat && !received.IsSigned)
            {
                Logger.Warn(
                    $"ReceivingPMode {receivingPMode.Id} is configured to reply with Non-Repudation Receipts, "
                    + $"but incoming UserMessage {userMessage.MessageId} isn\'t signed. "
                    + "This means that the Receipt cannot be created as a Non-Repudiation Receipt "
                    + "but in a Receipt with the referenced UserMessage embedded instead");
            }
            else if (!useNRRFormat)
            {
                Logger.Trace(
                    $"ReceivingPMode {receivingPMode?.Id} is configured to not use the Non-Repudiation format."
                    + $"This means the original UserMessage {userMessage.MessageId} will be included in the Receipt");
            }

            if (received.IsMultiHopMessage)
            {
                Logger.Trace(
                    $"Because the received UserMessage {userMessage.MessageId} has been sent via MultiHop, the Receipt will be send with MultiHop as well");
            }

            if (useNRRFormat && received.IsSigned)
            {
                    Logger.Trace($"ReceivingPMode {receivingPMode.Id} is configured to use Non-Repudiation for Receipt Creation");
                    return Receipt.CreateFor(
                        IdentifierFactory.Instance.Create(), 
                        userMessage,
                        received.SecurityHeader, 
                        received.IsMultiHopMessage);
            }

            return Receipt.CreateFor(
                IdentifierFactory.Instance.Create(), 
                userMessage, 
                received.IsMultiHopMessage);
        }
    }
}