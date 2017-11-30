using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;

namespace AS4.ParserService.Infrastructure
{
    internal class StepProcessor
    {
        internal static async Task<MessagingContext> ExecuteStepsAsync(MessagingContext context, StepConfiguration stepConfig, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<IStep> steps = CreateSteps(stepConfig.NormalPipeline);
                StepResult result = await ExecuteSteps(steps, context, cancellationToken);

                bool weHaveAnyUnhappyPath = stepConfig.ErrorPipeline != null;
                if (result.Succeeded == false && weHaveAnyUnhappyPath && result.MessagingContext.Exception == null)
                {
                    IEnumerable<IStep> unhappySteps = CreateSteps(stepConfig.ErrorPipeline);
                    result = await ExecuteSteps(unhappySteps, result.MessagingContext, cancellationToken);
                }

                return result.MessagingContext;
            }
            catch(Exception ex)
            {
                return new MessagingContext(ex);
            }
        }

        private static IEnumerable<IStep> CreateSteps(Step[] pipeline)
        {
            if (pipeline != null)
            {
                return StepBuilder.FromSettings(pipeline).BuildSteps();
            }

            return Enumerable.Empty<IStep>();
        }

        private static async Task<StepResult> ExecuteSteps(
            IEnumerable<IStep> steps,
            MessagingContext context,
            CancellationToken cancellation)
        {
            StepResult result = StepResult.Success(context);

            var currentContext = context;

            foreach (IStep step in steps)
            {
                result = await step.ExecuteAsync(currentContext, cancellation).ConfigureAwait(false);

                if (result.CanProceed == false || result.Succeeded == false || result.MessagingContext?.Exception != null)
                {
                    return result;
                }

                if (result.MessagingContext != null && currentContext != result.MessagingContext)
                {
                    currentContext = result.MessagingContext;
                }
            }

            return result;
        }
    }
}