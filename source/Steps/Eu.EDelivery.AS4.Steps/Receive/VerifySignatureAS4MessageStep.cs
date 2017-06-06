using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Signing;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes how a <see cref="AS4Message"/> signature gets verified
    /// </summary>
    public class VerifySignatureAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Start verifying the Signature of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception when the signature cannot be verified</exception>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            PreConditions(messagingContext);

            if (MessageDoesNotNeedToBeVerified(messagingContext))
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            return await TryVerifyingSignature(messagingContext);
        }

        private static void PreConditions(MessagingContext messagingContext)
        {

            ReceivingProcessingMode pmode = messagingContext.ReceivingPMode;
            SigningVerification verification = pmode?.Security.SigningVerification;

            bool isMessageFailsTheRequiredSigning = verification?.Signature == Limit.Required && !messagingContext.AS4Message.IsSigned;
            bool isMessageFailedTheUnallowedSigning = verification?.Signature == Limit.NotAllowed && messagingContext.AS4Message.IsSigned;

            if (isMessageFailsTheRequiredSigning)
            {
                string description = $"Receiving PMode {pmode.Id} requires a Signed AS4 Message and the message is not";
                throw ThrowVerifySignatureAS4Exception(description, ErrorCode.Ebms0103, messagingContext);
            }

            if (!isMessageFailedTheUnallowedSigning) return;
            {
                string description = $"Receiving PMode {pmode.Id} doesn't allow a signed AS4 Message and the message is";
                throw ThrowVerifySignatureAS4Exception(description, ErrorCode.Ebms0103, messagingContext);
            }
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
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                Logger.Error(exception.StackTrace);

                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                    Logger.Error(exception.InnerException.StackTrace);
                }

                throw ThrowVerifySignatureAS4Exception(exception.Message, ErrorCode.Ebms0101, messagingContext, exception);
            }
        }

        private static async Task<StepResult> VerifySignature(MessagingContext messagingContext)
        {
            if (!IsValidSignature(messagingContext.AS4Message))
            {
                throw ThrowVerifySignatureAS4Exception("The Signature is invalid", ErrorCode.Ebms0101, messagingContext);
            }

            Logger.Info($"{messagingContext.Prefix} AS4 Message has a valid Signature present");

            foreach (Attachment attachment in messagingContext.AS4Message.Attachments)
            {
                attachment.ResetContentPosition();
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static bool IsValidSignature(AS4Message as4Message)
        {
            VerifyConfig options = CreateVerifyOptionsForAS4Message(as4Message);

            return as4Message.SecurityHeader.Verify(options);
        }

        private static VerifyConfig CreateVerifyOptionsForAS4Message(AS4Message as4Message)
        {
            return new VerifyConfig
            {
                Attachments = as4Message.Attachments
            };
        }

        private static AS4Exception ThrowVerifySignatureAS4Exception(
            string description, ErrorCode errorCode, MessagingContext messagingContext, Exception innerException = null)
        {
            description = messagingContext.Prefix + description;
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(messagingContext.AS4Message.MessageIds)
                .WithErrorCode(errorCode)
                .WithInnerException(innerException)
                .WithReceivingPMode(messagingContext.ReceivingPMode)
                .Build();
        }
    }
}