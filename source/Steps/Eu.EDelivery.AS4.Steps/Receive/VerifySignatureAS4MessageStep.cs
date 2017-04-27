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
        private readonly ILogger _logger;

        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifySignatureAS4MessageStep"/> class.
        /// Create a new Verify Signature AS4 Message Step,
        /// which will verify the Signature in the AS4 Message (if present)
        /// </summary>
        public VerifySignatureAS4MessageStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start verifying the Signature of the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception when the signature cannot be verified</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            _internalMessage = internalMessage;

            PreConditions();
            if (MessageDoesNotNeedToBeVerified())
            {
                return await StepResult.SuccessAsync(internalMessage);
            }

            return await TryVerifyingSignature();
        }

        private void PreConditions()
        {
            AS4Message as4Message = _internalMessage.AS4Message;
            ReceivingProcessingMode pmode = as4Message.ReceivingPMode;
            SigningVerification verification = pmode?.Security.SigningVerification;

            bool isMessageFailsTheRequiredSigning = verification?.Signature == Limit.Required && !as4Message.IsSigned;
            bool isMessageFailedTheUnallowedSigning = verification?.Signature == Limit.NotAllowed && as4Message.IsSigned;

            if (isMessageFailsTheRequiredSigning)
            {
                string description = $"Receiving PMode {pmode.Id} requires a Signed AS4 Message and the message is not";
                throw ThrowVerifySignatureAS4Exception(description, ErrorCode.Ebms0103);
            }

            if (!isMessageFailedTheUnallowedSigning) return;
            {
                string description = $"Receiving PMode {pmode.Id} doesn't allow a signed AS4 Message and the message is";
                throw ThrowVerifySignatureAS4Exception(description, ErrorCode.Ebms0103);
            }
        }

        private bool MessageDoesNotNeedToBeVerified()
        {
            return !_internalMessage.AS4Message.IsSigned ||
                    _internalMessage.AS4Message.ReceivingPMode?.Security
                       .SigningVerification.Signature == Limit.Ignored;
        }

        private async Task<StepResult> TryVerifyingSignature()
        {
            try
            {
                return await VerifySignature();
            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                throw ThrowVerifySignatureAS4Exception(exception.Message, ErrorCode.Ebms0101, exception);
            }
        }

        private async Task<StepResult> VerifySignature()
        {
            if (!IsValidSignature())
            {
                throw ThrowVerifySignatureAS4Exception("The Signature is invalid", ErrorCode.Ebms0101);
            }

            _logger.Info($"{_internalMessage.Prefix} AS4 Message has a valid Signature present");

            foreach (Attachment attachment in _internalMessage.AS4Message.Attachments)
            {
                attachment.ResetContentPosition();
            }

            return await StepResult.SuccessAsync(_internalMessage);
        }

        private bool IsValidSignature()
        {
            AS4Message as4Message = _internalMessage.AS4Message;
            VerifyConfig options = CreateVerifyOptionsForAS4Message();

            return as4Message.SecurityHeader.Verify(options);
        }

        private VerifyConfig CreateVerifyOptionsForAS4Message()
        {
            return new VerifyConfig
            {
                Attachments = _internalMessage.AS4Message.Attachments
            };
        }

        private AS4Exception ThrowVerifySignatureAS4Exception(
            string description, ErrorCode errorCode, Exception innerException = null)
        {
            description = _internalMessage.Prefix + description;
            _logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(_internalMessage.AS4Message.MessageIds)
                .WithErrorCode(errorCode)
                .WithInnerException(innerException)
                .WithReceivingPMode(_internalMessage.AS4Message.ReceivingPMode)
                .Build();
        }
    }
}