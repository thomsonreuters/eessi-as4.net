using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how a <see cref="AS4Message"/> signature gets verified
    /// </summary>
    [Description("Verifies if the signature of the AS4 Message is correct. Message verification is necessary to ensure that the authenticity of the message is intact.")]
    [Info("Verify signature of received AS4 Message")]
    public class VerifySignatureAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _storeExpression;
        private readonly IConfig _config;
        private readonly IAS4MessageBodyStore _bodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifySignatureAS4MessageStep"/> class.
        /// </summary>
        public VerifySignatureAS4MessageStep()
            : this(
                Registry.Instance.CreateDatastoreContext,
                Config.Instance,
                Registry.Instance.MessageBodyStore)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifySignatureAS4MessageStep" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="bodyStore">The body store.</param>
        public VerifySignatureAS4MessageStep(
            Func<DatastoreContext> createContext,
            IConfig config,
            IAS4MessageBodyStore bodyStore)
        {
            _storeExpression = createContext;
            _config = config;
            _bodyStore = bodyStore;
        }

        /// <summary>
        /// Start verifying the Signature of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            ReceivingProcessingMode pmode = messagingContext.ReceivingPMode;
            SigningVerification verification = pmode?.Security.SigningVerification;
            AS4Message as4Message = messagingContext.AS4Message;

            bool isMessageFailsTheRequiredSigning = verification?.Signature == Limit.Required && !as4Message.IsSigned;
            bool isMessageFailedTheUnallowedSigning = verification?.Signature == Limit.NotAllowed && as4Message.IsSigned;

            if (isMessageFailsTheRequiredSigning)
            {
                string description = $"Receiving PMode {pmode.Id} requires a Signed AS4 Message and the message is not";
                return InvalidSignatureResult(description, ErrorAlias.PolicyNonCompliance, messagingContext);
            }

            if (isMessageFailedTheUnallowedSigning)
            {
                string description = $"Receiving PMode {pmode.Id} doesn't allow a signed AS4 Message and the message is";
                return InvalidSignatureResult(description, ErrorAlias.PolicyNonCompliance, messagingContext);
            }

            if (MessageDoesNotNeedToBeVerified(messagingContext))
            {
                return StepResult.Success(messagingContext);
            }

            if (as4Message.IsSignalMessage &&
                (messagingContext.SendingPMode?.ReceiptHandling?.VerifyNRR ?? true))
            {
                if (!await VerifyNonRepudiationHashes(as4Message))
                {
                    return InvalidSignatureResult(
                        "The digest value in the Signature References of the referenced UserMessage " +
                        "doesn't match the References of the NRI of the incoming NRR Receipt",
                        ErrorAlias.FailedAuthentication,
                        messagingContext);
                }
            }

            return await TryVerifyingSignature(messagingContext).ConfigureAwait(false);
        }

        private async Task<bool> VerifyNonRepudiationHashes(AS4Message as4Message)
        {
            IEnumerable<Receipt> receipts = as4Message.SignalMessages
                .Where(m => m is Receipt r && r.NonRepudiationInformation != null)
                .Cast<Receipt>();

            IEnumerable<AS4Message> userMessages =
                (await ReferencedUserMessagesOf(receipts)).Where(m => m != null && m.IsSigned);

            if (receipts.All(nrrReceipt =>
            {
                AS4Message refUserMessage = userMessages.FirstOrDefault(u =>
                    u.GetPrimaryMessageId() == nrrReceipt.RefToMessageId);

                return refUserMessage == null
                    || nrrReceipt.VerifyNonRepudiationInfo(refUserMessage);
            }))
            {
                Logger.Info($"[{as4Message.GetPrimaryMessageId()}] Incoming Receipt has valid NRI References");
                return true;
            }

            Logger.Error($"[{as4Message.GetPrimaryMessageId()}] Incoming Receipt hasn't got valid NRI References");
            return false;
        }

        /// <summary>
        /// Referenceds the user messages of.
        /// </summary>
        /// <param name="receipts">The receipts.</param>
        /// <returns></returns>
        private async Task<IEnumerable<AS4Message>> ReferencedUserMessagesOf(IEnumerable<Receipt> receipts)
        {
            using (DatastoreContext context = _storeExpression())
            {
                var service = new OutMessageService(
                    _config, 
                    new DatastoreRepository(context),
                    _bodyStore);

                return await service.GetAS4UserMessagesForIds(receipts.Select(r => r.RefToMessageId), _bodyStore);
            }
        }

        private static bool MessageDoesNotNeedToBeVerified(MessagingContext message)
        {
            AS4Message as4Message = message.AS4Message;
            bool signatureIgnored = message.ReceivingPMode?.Security.SigningVerification.Signature == Limit.Ignored;

            return !as4Message.IsSigned || signatureIgnored;
        }

        private static async Task<StepResult> TryVerifyingSignature(MessagingContext messagingContext)
        {
            try
            {
                return await VerifySignature(messagingContext).ConfigureAwait(false);
            }
            catch (CryptographicException exception)
            {
                Logger.Error($"An exception occured while validating the signature: {exception.Message}");
                return InvalidSignatureResult(exception.Message, ErrorAlias.FailedAuthentication, messagingContext);
            }
        }

        private static async Task<StepResult> VerifySignature(MessagingContext messagingContext)
        {
            VerifySignatureConfig options = 
                CreateVerifyOptionsForAS4Message(messagingContext.AS4Message, messagingContext.ReceivingPMode);

            if (!messagingContext.AS4Message.VerifySignature(options))
            {
                const string description = "The signature is invalid";
                Logger.Error(description);
                return InvalidSignatureResult(description, ErrorAlias.FailedAuthentication, messagingContext);
            }

            Logger.Info($"{messagingContext.EbmsMessageId} AS4 Message has a valid Signature present");
            return await StepResult.SuccessAsync(messagingContext);
        }

        private static VerifySignatureConfig CreateVerifyOptionsForAS4Message(AS4Message as4Message, ReceivingProcessingMode pmode)
        {
            bool allowUnknownRootCertificateAuthority =
                pmode?.Security?.SigningVerification?.AllowUnknownRootCertificate
                ?? new ReceivingProcessingMode().Security.SigningVerification.AllowUnknownRootCertificate;

            return new VerifySignatureConfig(
                allowUnknownRootCertificateAuthority,
                as4Message.Attachments);
        }

        private static StepResult InvalidSignatureResult(string description, ErrorAlias errorAlias, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, errorAlias);
            return StepResult.Failed(context);
        }
    }
}