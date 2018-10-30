using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    internal class StepExecutioner
    {
        private readonly (ConditionalStepConfig happyPath, ConditionalStepConfig unhappyPath) _conditionalPipeline;
        private readonly StepConfiguration _stepConfiguration;
        private readonly IAgentExceptionHandler _exceptionHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="StepExecutioner"/> class.
        /// </summary>
        /// <param name="steps">The configuration used to build <see cref="IStep"/> instances.</param>
        /// <param name="handler">The handler used to handle exceptions during the step executions.</param>
        public StepExecutioner(
            StepConfiguration steps,
            IAgentExceptionHandler handler)
        {
            if (steps == null)
            {
                throw new ArgumentNullException(nameof(steps));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _stepConfiguration = steps;
            _exceptionHandler = handler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepExecutioner"/> class.
        /// </summary>
        /// <param name="conditionalSteps">The configuration used to build <see cref="IStep"/> instances.</param>
        /// <param name="handler">The handler used to handle exceptions during the step executions.</param>
        internal StepExecutioner(
            (ConditionalStepConfig thenSteps, ConditionalStepConfig elseSteps) conditionalSteps, 
            IAgentExceptionHandler handler)
        {
            if (conditionalSteps.thenSteps == null)
            {
                throw new ArgumentNullException(nameof(conditionalSteps.thenSteps));
            }

            if (conditionalSteps.elseSteps == null)
            {
                throw new ArgumentNullException(nameof(conditionalSteps.elseSteps));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _conditionalPipeline = conditionalSteps;
            _exceptionHandler = handler;
        }

        /// <summary>
        /// Run through all the configured steps using the given <paramref name="currentContext"/> as input.
        /// </summary>
        /// <param name="currentContext">The input that gets passed to the step pipeline.</param>
        /// <returns>The result of the last-executed step from the normal or error pipeline if there hasn't been an exception occured.</returns>
        public async Task<MessagingContext> ExecuteStepsAsync(MessagingContext currentContext)
        {
            bool hasNoStepsConfigured = 
                _conditionalPipeline.happyPath == null
                && (_stepConfiguration.NormalPipeline.Any(s => s == null) 
                    || _stepConfiguration.NormalPipeline == null);

            if (hasNoStepsConfigured)
            {
                return currentContext;
            }

            StepResult result = StepResult.Success(currentContext);

            try
            {
                IEnumerable<IStep> steps = CreateSteps(_stepConfiguration?.NormalPipeline, _conditionalPipeline.happyPath);
                result = await ExecuteStepsAsync(steps, currentContext);
            }
            catch (Exception exception)
            {
                return await _exceptionHandler.HandleExecutionException(exception, currentContext);
            }

            try
            {
                bool weHaveAnyUnhappyPath = 
                    _stepConfiguration?.ErrorPipeline != null 
                    || _conditionalPipeline.unhappyPath != null;

                if (result.Succeeded == false 
                    && weHaveAnyUnhappyPath 
                    && result.MessagingContext.Exception == null)
                {
                    IEnumerable<IStep> steps = CreateSteps(_stepConfiguration?.ErrorPipeline, _conditionalPipeline.unhappyPath);
                    result = await ExecuteStepsAsync(steps, result.MessagingContext);
                }

                return result.MessagingContext;
            }
            catch (Exception exception)
            {
                return await _exceptionHandler.HandleErrorException(exception, result.MessagingContext);
            }
        }

        private static IEnumerable<IStep> CreateSteps(Step[] pipeline, ConditionalStepConfig conditionalConfig)
        {
            if (pipeline != null)
            {
                return StepBuilder.FromSettings(pipeline).BuildSteps();
            }

            if (conditionalConfig != null)
            {
                return StepBuilder.FromConditionalConfig(conditionalConfig).BuildSteps();
            }

            return Enumerable.Empty<IStep>();
        }

        private static async Task<StepResult> ExecuteStepsAsync(
            IEnumerable<IStep> steps,
            MessagingContext context)
        {
            StepResult result = StepResult.Success(context);
            MessagingContext currentContext = context;

            foreach (IStep step in steps)
            {
                result = await step.ExecuteAsync(currentContext).ConfigureAwait(false);

                if (result == null)
                {
                    throw new InvalidOperationException(
                        $"Result of last step: {step.GetType().Name} returns 'null'");
                }

                if (result.MessagingContext == null)
                {
                    throw new InvalidOperationException(
                        $"Result of last step {step.GetType().Name} doesn't have a 'MessagingContext'");
                }

                if (result.CanProceed == false 
                    || result.Succeeded == false 
                    || result.MessagingContext.Exception != null)
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
