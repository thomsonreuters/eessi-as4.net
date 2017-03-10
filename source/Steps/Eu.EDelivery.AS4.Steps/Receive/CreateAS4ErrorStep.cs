using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Create an <see cref="Error"/> 
    /// from a given <see cref="AS4Exception"/>
    /// </summary>
    public class CreateAS4ErrorStep : IStep
    {
        private readonly ILogger _logger;

        private AS4Message _originalAS4Message;

        /// <summary>
        /// Initializes a new intance of the type <see cref="CreateAS4ErrorStep"/> class
        /// </summary>
        public CreateAS4ErrorStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start creating <see cref="Error"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (IsEmptySoapBody(internalMessage))
                return await StepResult.SuccessAsync(internalMessage);

            this._originalAS4Message = internalMessage.AS4Message;
            return await ReturnAS4ErrorMessage(internalMessage);
        }

        private async Task<StepResult> ReturnAS4ErrorMessage(InternalMessage internalMessage)
        {
            this._logger.Info($"{internalMessage.Prefix} Create AS4 Error Message from AS4 Exception");

            AS4Message errorMessage = CreateErrorAS4Message(internalMessage.Exception);
            errorMessage.SigningId = this._originalAS4Message.SigningId;
            errorMessage.SendingPMode = this._originalAS4Message.SendingPMode;
            var errorInternalMessage = new InternalMessage(errorMessage);

            return await StepResult.SuccessAsync(errorInternalMessage);
        }

        private static bool IsEmptySoapBody(InternalMessage internalMessage)
        {
            return internalMessage.Exception == null;
        }

        private AS4Message CreateErrorAS4Message(AS4Exception exception)
        {
            Error error = CreateError(exception);
            return CreateMessage(error);
        }

        private AS4Message CreateMessage(SignalMessage error)
        {
            AS4MessageBuilder builder = new AS4MessageBuilder().WithSignalMessage(error);
            ReceivingProcessingMode receivingPMode = this._originalAS4Message.ReceivingPMode;
            if (receivingPMode != null)
            {
                builder.WithReceivingPMode(receivingPMode);
            }

            return builder.Build();
        }

        private Error CreateError(AS4Exception exception)
        {
            string messageId = GetMessageId();

            return new ErrorBuilder()
                .WithRefToEbmsMessageId(messageId)
                .WithAS4Exception(exception)
                .Build();
        }

        private string GetMessageId()
        {
            return this._originalAS4Message.PrimaryUserMessage?.MessageId ??
                   this._originalAS4Message.PrimarySignalMessage.MessageId;
        }
    }
}