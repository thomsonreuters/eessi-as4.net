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
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {

            if (messagingContext.AS4Message.IsEmpty)
            {
                return await ReturnSameStepResult(messagingContext);
            }

            return  IsReplyPatternCallback(messagingContext)
                ? await CreateEmptySoapResult(messagingContext)
                : await ReturnSameStepResult(messagingContext);
        }

        private static bool IsReplyPatternCallback(MessagingContext message)
        {
            return message.ReceivingPMode?.ReceiptHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private async Task<StepResult> CreateEmptySoapResult(MessagingContext messagingContext)
        {
            _logger.Info($"{messagingContext.Prefix} Empty SOAP Envelope will be send to requested party");

            var emptyInternalMessage = new MessagingContext(CreateEmptyAS4Message(messagingContext.SendingPMode))
            {
                ReceivingPMode = messagingContext.ReceivingPMode
            };

            return await StepResult.SuccessAsync(emptyInternalMessage);
        }

        private static AS4Message CreateEmptyAS4Message(SendingProcessingMode pmode)
        {            
            return new AS4MessageBuilder(pmode).Build();
        }

        private static async Task<StepResult> ReturnSameStepResult(MessagingContext messagingContext)
        {            
            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}