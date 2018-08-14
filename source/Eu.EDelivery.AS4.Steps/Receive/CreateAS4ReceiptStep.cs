using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using NLog;
using NonRepudiationInformation = Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation;
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
        private readonly IConfig _config;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ReceiptStep"/> class.
        /// </summary>
        public CreateAS4ReceiptStep() : this(Config.Instance) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ReceiptStep"/> class.
        /// </summary>
        /// <param name="config"></param>
        public CreateAS4ReceiptStep(IConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _config = config;
        }

        /// <summary>
        /// It is only executed when the external message (received) is an AS4 UserMessage
        /// </summary>
        /// <param name="messagingContext"></param>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            AS4Message receivedAS4Message = messagingContext?.AS4Message;
            if (receivedAS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(CreateAS4ReceiptStep)} requires an AS4Message to create a Receipt for but no AS4Message is present in the MessagingContext");
            }

            SendingProcessingMode responseSendPMode =
                messagingContext.SendingPMode 
                ?? messagingContext.GetReferencedSendingPMode(messagingContext.ReceivingPMode, _config);

            if (responseSendPMode == null)
            {
                Logger.Error(
                    "Failed to create Receipt response because no SendingPMode can be determined for the creation of the response, " + 
                    "this can happen when the Receiving Processing Mode doesn't reference an exising Sending Processing Mode in the ReplyHandling.SendingPMode element");

                messagingContext.ErrorResult = 
                    new ErrorResult(
                        "Failed to create Receipt response because no Sending Processing Mode can be determined for the creation of the response", 
                        ErrorAlias.ProcessingModeMismatch);

                return StepResult.Failed(messagingContext);
            }

            AS4Message receiptMessage = AS4Message.Create(responseSendPMode);
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
            messagingContext.SendingPMode = responseSendPMode;

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

            if (useNRRFormat && received.IsSigned)
            {
                    Logger.Debug(
                        $"ReceivingPMode {receivingPMode?.Id} is configured to use Non-Repudiation for Receipt Creation");

                    var nonRepudiation = new NonRepudiationInformation(
                        received.SecurityHeader
                                .GetReferences()
                                .Select(Reference.CreateFromReferenceElement));

                    return GetRoutingInfoForUserMessage(userMessage, received.IsMultiHopMessage)
                        .Select(routing => new Receipt(userMessage.MessageId, nonRepudiation, routing))
                        .GetOrElse(() => new Receipt(userMessage.MessageId, nonRepudiation));
            }

            return GetRoutingInfoForUserMessage(userMessage, received.IsMultiHopMessage)
                .Select(routing => new Receipt(userMessage.MessageId, userMessage, routing))
                .GetOrElse(() => new Receipt(userMessage.MessageId, userMessage));
        }

        private static Maybe<RoutingInputUserMessage> GetRoutingInfoForUserMessage(UserMessage userMessage, bool isMultihop)
        {
            return isMultihop.ThenMaybe(() =>
            {
                Logger.Debug(
                    $"Because the received UserMessage {userMessage.MessageId} has been sent via MultiHop, the Receipt will be send as MultiHop also");

                return AS4Mapper.Map<RoutingInputUserMessage>(userMessage);
            });
        }
    }
}