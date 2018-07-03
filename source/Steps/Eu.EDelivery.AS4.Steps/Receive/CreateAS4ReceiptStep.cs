using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
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
using Reference = System.Security.Cryptography.Xml.Reference;

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
            _config = config;
        }

        /// <summary>
        /// It is only executed when the external message (received) is an AS4 UserMessage
        /// </summary>
        /// <param name="messagingContext"></param>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            SendingProcessingMode responseSendPMode =
                messagingContext.SendingPMode 
                ?? messagingContext.GetReferencedSendingPMode(messagingContext.ReceivingPMode, _config);

            if (responseSendPMode == null)
            {
                const string desc = "Cannot determine a Sending Processing Mode during the creation of the response";
                Logger.Error(desc);

                messagingContext.ErrorResult = new ErrorResult(desc, ErrorAlias.ProcessingModeMismatch);
                return StepResult.Failed(messagingContext);
            }

            AS4Message receivedAS4Message = messagingContext.AS4Message;
            
            // Should we create a Receipt for each and every UserMessage that can be present in the bundle ?
            // If no UserMessages are present, an Empty AS4Message should be returned.
            AS4Message receiptMessage = AS4Message.Create(responseSendPMode);
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
            messagingContext.SendingPMode = responseSendPMode;

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static Receipt CreateReferencedReceipt(string ebmsMessageId, MessagingContext messagingContext)
        {
            var receipt = new Receipt(ebmsMessageId);
            AS4Message as4Message = messagingContext.AS4Message;
            bool useNRRFormat = messagingContext.ReceivingPMode?.ReplyHandling.ReceiptHandling.UseNRRFormat ?? false;

            if (useNRRFormat && !as4Message.IsSigned)
            {
                Logger.Warn(
                    $"{messagingContext.LogTag} Receiving PMode ({messagingContext.ReceivingPMode?.Id}) " +
                    "is configured to reply with Non-Repudation Receipts, but incoming UserMessage isn't signed");
            }
            else if (!useNRRFormat)
            {
                Logger.Debug(
                    $"{messagingContext.LogTag} Receiving PMode is configured to not use the Non-Repudiation format." +
                    "This means the original UserMessage will be included in the Receipt");
            }

            if (useNRRFormat && as4Message.IsSigned)
            {
                    Logger.Debug(
                        $"{messagingContext.LogTag} Receiving PMode {messagingContext.ReceivingPMode?.Id} " + 
                        $"is configured to use Non-Repudiation for Receipt {receipt.MessageId} Creation");

                    NonRepudiationInformation nonRepudiation = 
                        GetNonRepudiationInformationFrom(as4Message);

                    return GetRoutingInfoForUserMessage(as4Message)
                        .Select(routing => new Receipt(ebmsMessageId, nonRepudiation, routing))
                        .GetOrElse(() => new Receipt(ebmsMessageId, nonRepudiation));
            }

            return GetRoutingInfoForUserMessage(as4Message)
                   .Select(routing => new Receipt(ebmsMessageId, as4Message.FirstUserMessage, routing))
                   .GetOrElse(() => new Receipt(ebmsMessageId, as4Message.FirstUserMessage));
        }

        private static Maybe<RoutingInputUserMessage> GetRoutingInfoForUserMessage(AS4Message msg)
        {
            if (msg.IsMultiHopMessage)
            {
                Logger.Debug(
                    "Because the received UserMessage has been sent via MultiHop, " +
                    "we will send the Receipt as MultiHop also");

                return Maybe.Just(
                    AS4Mapper.Map<RoutingInputUserMessage>(msg.FirstUserMessage));
            }

            return Maybe<RoutingInputUserMessage>.Nothing;
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