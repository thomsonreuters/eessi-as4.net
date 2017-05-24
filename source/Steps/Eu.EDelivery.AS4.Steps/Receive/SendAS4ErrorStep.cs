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
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (internalMessage.AS4Message?.IsEmpty == true)
            {
                return await ReturnSameStepResult(internalMessage);
            }

            return IsReplyPatternCallback(internalMessage)
                ? await CreateEmptySoapResult(internalMessage)
                : await ReturnSameStepResult(internalMessage);
        }

        private static bool IsReplyPatternCallback(InternalMessage message)
        {
            return message?.ReceivingPMode?.ErrorHandling.ReplyPattern == ReplyPattern.Callback;
        }

        private static async Task<StepResult> CreateEmptySoapResult(InternalMessage internalMessage)
        {
            LogManager.GetCurrentClassLogger().Info($"{internalMessage.Prefix} Empty SOAP Envelope will be send to requested party");

            AS4Message emptyAS4Message = CreateEmptyAS4Message(internalMessage.ReceivingPMode);
            var emptyInternalMessage = new InternalMessage(emptyAS4Message);
            return await StepResult.SuccessAsync(emptyInternalMessage);
        }

        private static AS4Message CreateEmptyAS4Message(ReceivingProcessingMode receivingPMode)
        {
            return new AS4MessageBuilder().WithReceivingPMode(receivingPMode).Build();
        }

        private static async Task<StepResult> ReturnSameStepResult(InternalMessage internalMessage)
        {
            return await StepResult.SuccessAsync(internalMessage);
        }
    }
}