using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class SendAS4SignalMessageStep : IStep
    {
        /// <summary>
        /// Start executing the Receipt Decorator
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message.IsEmpty)
            {
                return await ReturnSameStepResult(messagingContext);
            }

            return IsReplyPatternCallback(messagingContext)
                       ? await CreateEmptySoapResult(messagingContext)
                       : await ReturnSameStepResult(messagingContext);
        }

        private static bool IsReplyPatternCallback(MessagingContext message)
        {
            return message.ReceivingPMode?.ReplyHandling?.ReplyPattern == ReplyPattern.Callback;
        }

        private static async Task<StepResult> CreateEmptySoapResult(MessagingContext messagingContext)
        {
            LogManager.GetCurrentClassLogger()
                      .Info($"{messagingContext.Prefix} Empty SOAP Envelope will be send to requested party");

            AS4Message as4Message = AS4Message.Create(messagingContext.SendingPMode);
            var emptyContext = new MessagingContext(as4Message, MessagingContextMode.Receive)
            {
                ReceivingPMode = messagingContext.ReceivingPMode
            };

            return await StepResult.SuccessAsync(emptyContext);
        }

        private static async Task<StepResult> ReturnSameStepResult(MessagingContext messagingContext)
        {
            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}
