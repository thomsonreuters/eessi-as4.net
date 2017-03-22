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

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4ReceiptStep"/> class
        /// to send <see cref="Receipt"/> Messages
        /// </summary>
        public SendAS4ReceiptStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start executing the Receipt Decorator
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {

            if (internalMessage.AS4Message.IsEmpty)
            {
                return await ReturnSameStepResult(internalMessage);
            }

            return  IsReplyPatternCallback(internalMessage.AS4Message)
                ? await CreateEmptySoapResult(internalMessage)
                : await ReturnSameStepResult(internalMessage);
        }

        private bool IsReplyPatternCallback(AS4Message as4Message)
        {
            return as4Message.ReceivingPMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private async Task<StepResult> CreateEmptySoapResult(InternalMessage internalMessage)
        {
            _logger.Info($"{internalMessage.Prefix} Empty SOAP Envelope will be send to requested party");

            AS4Message emptyAS4Message = CreateEmptyAS4Message(internalMessage.AS4Message.ReceivingPMode);
            var emptyInternalMessage = new InternalMessage(emptyAS4Message);
            return await StepResult.SuccessAsync(emptyInternalMessage);
        }

        private AS4Message CreateEmptyAS4Message(ReceivingProcessingMode receivingPMode)
        {            
            return new AS4MessageBuilder().WithReceivingPMode(receivingPMode).Build();
        }

        private async Task<StepResult> ReturnSameStepResult(InternalMessage internalMessage)
        {            
            return await StepResult.SuccessAsync(internalMessage);
        }
    }
}