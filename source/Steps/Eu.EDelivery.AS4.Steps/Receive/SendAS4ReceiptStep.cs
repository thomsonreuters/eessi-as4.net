using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Describes where the created Receipt must be placed
    /// </summary>
    public class SendAS4ReceiptStep : IStep
    {
        private readonly ILogger _logger;
        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4ReceiptStep"/> class
        /// to send <see cref="Receipt"/> Messages
        /// </summary>
        public SendAS4ReceiptStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start executing the Receipt Decorator
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._internalMessage = internalMessage;

            return IsReplyPatternCallback()
                ? CreateEmptySoapResult()
                : ReturnSameStepResult();
        }

        private bool IsReplyPatternCallback()
        {
            return this._internalMessage.AS4Message
                .ReceivingPMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private Task<StepResult> CreateEmptySoapResult()
        {
            this._logger.Info($"{this._internalMessage.Prefix} Empty SOAP Envelope will be send to requested party");

            AS4Message emptyAS4Message = CreateEmptyAS4Message();
            var emptyInternalMessage = new InternalMessage(emptyAS4Message);
            return StepResult.SuccessAsync(emptyInternalMessage);
        }

        private AS4Message CreateEmptyAS4Message()
        {
            ReceivingProcessingMode pmode = this._internalMessage.AS4Message.ReceivingPMode;
            return new AS4MessageBuilder().WithReceivingPMode(pmode).Build();
        }

        private Task<StepResult> ReturnSameStepResult()
        {
            string messageId = this._internalMessage.AS4Message.PrimarySignalMessage.MessageId;
            this._logger.Info($"{this._internalMessage.Prefix} Receipt {messageId} is prepared to send to requested party");

            return StepResult.SuccessAsync(this._internalMessage);
        }
    }
}