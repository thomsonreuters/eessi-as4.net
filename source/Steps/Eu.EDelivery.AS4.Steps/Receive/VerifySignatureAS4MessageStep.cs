using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Services;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using NLog;
using Reference = System.Security.Cryptography.Xml.Reference;

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

            if (as4Message.IsSignalMessage
                && !as4Message.IsMultiHopMessage
                && messagingContext.SendingPMode.ReceiptHandling.VerifyNRR)
            {
                if (!await VerifyNonRepudiationsHashes(as4Message))
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

        private async Task<bool> VerifyNonRepudiationsHashes(AS4Message as4Message)
        {
            using (DatastoreContext context = _storeExpression())
            {
                var service = new OutMessageService(_config, new DatastoreRepository(context), _bodyStore);
                foreach (Receipt nrrReceipt in as4Message.SignalMessages.Where(m => m is Receipt).Cast<Receipt>())
                {
                    AS4Message referencedUserMessage = await service.GetAS4UserMessageForId(
                        nrrReceipt.RefToMessageId,
                        _bodyStore);

                    if (!referencedUserMessage.IsSigned) { continue; }

                    IEnumerable<Reference> signedReferences = referencedUserMessage
                        .SecurityHeader.GetReferences()
                        .ToArray().ToList().Cast<Reference>();

                    if (!nrrReceipt.VerifyNonRepudiations(signedReferences))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool MessageDoesNotNeedToBeVerified(MessagingContext message)
        {
            AS4Message as4Message = message.AS4Message;

            return !as4Message.IsSigned ||
                    message.ReceivingPMode?.Security.SigningVerification.Signature == Limit.Ignored;
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
            if (!IsValidSignature(messagingContext.AS4Message, CreateVerifyOptionsForAS4Message(messagingContext.AS4Message, messagingContext.ReceivingPMode)))
            {
                string description = "The signature is invalid";
                Logger.Error(description);
                return InvalidSignatureResult(description, ErrorAlias.FailedAuthentication, messagingContext);
            }

            Logger.Info($"{messagingContext.EbmsMessageId} AS4 Message has a valid Signature present");

            foreach (Attachment attachment in messagingContext.AS4Message.Attachments)
            {
                attachment.ResetContentPosition();
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static bool IsValidSignature(AS4Message as4Message, VerifySignatureConfig options)
        {
            return as4Message.SecurityHeader.Verify(options);
        }

        private static VerifySignatureConfig CreateVerifyOptionsForAS4Message(AS4Message as4Message, ReceivingProcessingMode pmode)
        {
            return new VerifySignatureConfig
            {
                Attachments = as4Message.Attachments,
                AllowUnknownRootCertificateAuthority =
                    pmode?.Security?.SigningVerification?.AllowUnknownRootCertificate 
                    ?? new ReceivingProcessingMode().Security.SigningVerification.AllowUnknownRootCertificate
            };
        }

        private static StepResult InvalidSignatureResult(string description, ErrorAlias errorAlias, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, errorAlias);
            return StepResult.Failed(context);
        }
    }
}