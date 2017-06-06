using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class SendAS4ErrorStep : IStep
    {        
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message?.IsEmpty == true)
            {
                return await ReturnSameStepResult(messagingContext);
            }

            return IsReplyPatternCallback(messagingContext)
                ? await CreateEmptySoapResult(messagingContext)
                : await ReturnSameStepResult(messagingContext);
        }

        private static bool IsReplyPatternCallback(MessagingContext message)
        {
            return message?.ReceivingPMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private static async Task<StepResult> CreateEmptySoapResult(MessagingContext messagingContext)
        {
            LogManager.GetCurrentClassLogger().Info($"{messagingContext.Prefix} Empty SOAP Envelope will be send to requested party");

            var emptyInternalMessage = new MessagingContext(CreateEmptyAS4Message())
            {
                ReceivingPMode = messagingContext.ReceivingPMode
            };

            return await StepResult.SuccessAsync(emptyInternalMessage);
        }

        private static AS4Message CreateEmptyAS4Message()
        {
            return new AS4MessageBuilder().Build();
        }

        private static async Task<StepResult> ReturnSameStepResult(MessagingContext messagingContext)
        {
            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}