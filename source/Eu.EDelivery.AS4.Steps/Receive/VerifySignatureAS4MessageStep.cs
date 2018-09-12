using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
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
    [Info("Verify signature of received AS4 Message")]
    [Description(
        "Verifies if the signature of the AS4 Message is correct. " + 
        "Message verification is necessary to ensure that the authenticity of the message is intact.")]
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
                Registry.Instance.MessageBodyStore) { }

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
            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (bodyStore == null)
            {
                throw new ArgumentNullException(nameof(bodyStore));
            }

            _storeExpression = createContext;
            _config = config;
            _bodyStore = bodyStore;
        }

        /// <summary>
        /// Start verifying the Signature of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext?.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(VerifySignatureAS4MessageStep)} requires an AS4Message to verify but no AS4Message is present in the MessagingContext");
            }

            SigningVerification verification = DetermineSigningVerification(messagingContext);
            if (verification == null)
            {
                Logger.Debug("No PMode.Security.SigningVerification element found, so no signature verification will take place");
                return StepResult.Success(messagingContext);
            }

            AS4Message as4Message = messagingContext.AS4Message;
            (bool unsignedButRequired, string desRequired) = SigningRequiredRule(verification, as4Message);
            if (unsignedButRequired)
            {
                return InvalidSignatureResult(
                    desRequired, ErrorAlias.PolicyNonCompliance, messagingContext);
            }

            (bool signedMessageButUnallowed, string desUnallowed) = SigningUnallowedRule(verification, as4Message);
            if (signedMessageButUnallowed)
            {
                return InvalidSignatureResult(
                    desUnallowed, ErrorAlias.PolicyNonCompliance, messagingContext);
            }

            if (!as4Message.IsSigned || verification?.Signature == Limit.Ignored)
            {
                Logger.Debug(
                    "No signature verification will take place for unsiged messages " + 
                    "or PModes that has a SigningVerification=Ignored");

                return StepResult.Success(messagingContext);
            }

            if (as4Message.MessageUnits.Any(u => u is Receipt) &&
                (messagingContext.SendingPMode?.ReceiptHandling?.VerifyNRR ?? true))
            {
                if (!await VerifyNonRepudiationHashes(as4Message))
                {
                    Logger.Error($"{messagingContext.LogTag} Incoming Receipt hasn't got valid NRI References");

                    return InvalidSignatureResult(
                        "The digest value in the Signature References of the referenced UserMessage " +
                        "doesn't match the References of the NRI of the incoming NRR Receipt",
                        ErrorAlias.FailedAuthentication,
                        messagingContext);
                }

                Logger.Debug($"{messagingContext.LogTag} Incoming Receipt has valid NRI References");
            }

            return await TryVerifyingSignature(messagingContext, verification).ConfigureAwait(false);
        }

        private static SigningVerification DetermineSigningVerification(MessagingContext ctx)
        {
            if (ctx.AS4Message.IsSignalMessage
                && !ctx.AS4Message.IsMultiHopMessage)
            {
                Logger.Debug($"Use SendingPMode {ctx.SendingPMode?.Id} for signature verification");
                return ctx.SendingPMode?.Security?.SigningVerification;
            }

            Logger.Debug($"Use ReceivingPMode {ctx.ReceivingPMode?.Id} for signature verification");
            return ctx.ReceivingPMode?.Security?.SigningVerification;
        }

        private static (bool, string) SigningRequiredRule(SigningVerification v, AS4Message m)
        {
            return (
                v.Signature == Limit.Required && !m.IsSigned, 
                "PMode requires a signed AS4Message but the received AS4message is not signed");
        }

        private static (bool, string) SigningUnallowedRule(SigningVerification v, AS4Message m)
        {
            return (
                v.Signature == Limit.NotAllowed && m.IsSigned,
                "PMode doesn't allow a signed AS4Message and the received AS4Message is signed");
        }

        private async Task<bool> VerifyNonRepudiationHashes(AS4Message as4Message)
        {
            IEnumerable<Receipt> receipts = as4Message.SignalMessages
                .Where(m => m is Receipt r && r.NonRepudiationInformation != null)
                .Cast<Receipt>();

            IEnumerable<AS4Message> userMessages =
                (await ReferencedUserMessagesOf(receipts)).Where(m => m != null && m.IsSigned);

            if (!userMessages.Any())
            {
                Logger.Debug("No UserMessage(s) are found for the incoming Receipt(s)");
            }

            return receipts.All(nrrReceipt =>
            {
                AS4Message refUserMessage = userMessages.FirstOrDefault(
                    u => u.GetPrimaryMessageId() == nrrReceipt.RefToMessageId);

                return refUserMessage == null
                       || nrrReceipt.VerifyNonRepudiationInfo(refUserMessage);
            });
        }

        private async Task<IEnumerable<AS4Message>> ReferencedUserMessagesOf(IEnumerable<Receipt> receipts)
        {
            using (DatastoreContext context = _storeExpression())
            {
                var service = new OutMessageService(
                    repository: new DatastoreRepository(context),
                    messageBodyStore: _bodyStore);

                return await service.GetNonIntermediaryAS4UserMessagesForIds(
                    receipts.Select(r => r.RefToMessageId));
            }
        }

        private static async Task<StepResult> TryVerifyingSignature(
            MessagingContext messagingContext,
            SigningVerification verification)
        {
            try
            {
                VerifySignatureConfig options =
                    CreateVerifyOptionsForAS4Message(messagingContext.AS4Message, verification);

                Logger.Debug($"Verify signature on the AS4Message {{AllowUnknownRootCertificateAuthority={options.AllowUnknownRootCertificateAuthority}}}");
                if (!messagingContext.AS4Message.VerifySignature(options))
                {
                    return InvalidSignatureResult(
                        "The signature is invalid",
                        ErrorAlias.FailedAuthentication,
                        messagingContext);
                }

                Logger.Info($"{messagingContext.LogTag} AS4Message has a valid Signature present");
                return await StepResult.SuccessAsync(messagingContext);
            }
            catch (CryptographicException exception)
            {
                Logger.Error($"{messagingContext.LogTag} An exception occured while validating the signature: {exception.Message}");
                return InvalidSignatureResult(
                    exception.Message, 
                    ErrorAlias.FailedAuthentication, 
                    messagingContext);
            }
        }

        private static VerifySignatureConfig CreateVerifyOptionsForAS4Message(AS4Message as4Message, SigningVerification v)
        {
            bool allowUnknownRootCertificateAuthority = 
                v?.AllowUnknownRootCertificate
                ?? new SigningVerification().AllowUnknownRootCertificate;

            return new VerifySignatureConfig(
                allowUnknownRootCertificateAuthority,
                as4Message.Attachments);
        }

        private static StepResult InvalidSignatureResult(string description, ErrorAlias errorAlias, MessagingContext context)
        {
            Logger.Error(description);

            context.ErrorResult = new ErrorResult(description, errorAlias);
            return StepResult.Failed(context);
        }
    }
}