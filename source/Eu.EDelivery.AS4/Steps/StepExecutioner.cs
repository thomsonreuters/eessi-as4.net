using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.Journal;

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
        public async Task<StepResult> ExecuteStepsAsync(MessagingContext currentContext)
        {
            bool hasNoStepsConfigured = 
                _conditionalPipeline.happyPath == null
                && (_stepConfiguration.NormalPipeline.Any(s => s == null) 
                    || _stepConfiguration.NormalPipeline == null);

            if (hasNoStepsConfigured)
            {
                return StepResult.Success(currentContext);
            }

            StepResult result = StepResult.Success(currentContext);

            try
            {
                IEnumerable<IStep> steps = CreateSteps(_stepConfiguration?.NormalPipeline, _conditionalPipeline.happyPath);
                result = await ExecuteStepsAsync(steps, result);
            }
            catch (Exception exception)
            {
                MessagingContext handled = 
                    await _exceptionHandler.HandleExecutionException(exception, currentContext);

                return StepResult.Failed(handled);
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
                    result = await ExecuteStepsAsync(steps, result);
                }

                return result;
            }
            catch (Exception exception)
            {
                MessagingContext handled = 
                    await _exceptionHandler.HandleErrorException(exception, result.MessagingContext);

                return StepResult.Failed(handled);
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
            StepResult initialResult)
        {
            StepResult lastResult = initialResult;
            MessagingContext currentContext = lastResult.MessagingContext;

            ICollection<JournalLogEntry> journal = lastResult.Journal.ToList();

            foreach (IStep step in steps)
            {
                StepResult nextResult = await ExecuteStepAsync(currentContext, step);

                AddOrUpdateJournal(journal, nextResult);

                if (nextResult.CanProceed == false
                    || nextResult.Succeeded == false
                    || nextResult.MessagingContext.Exception != null)
                {
                    return nextResult.WithJournal(journal);
                }

                if (nextResult.MessagingContext != null 
                    && currentContext != nextResult.MessagingContext)
                {
                    currentContext = nextResult.MessagingContext;
                }

                lastResult = nextResult;
            }

            return lastResult.WithJournal(journal);
        }

        private static void AddOrUpdateJournal(ICollection<JournalLogEntry> journal, StepResult nextResult)
        {
            foreach (JournalLogEntry entry in nextResult.Journal)
            {
                JournalLogEntry existed =
                    journal.FirstOrDefault(j => JournalLogEntryComparer.ByEbmsMessageId.Equals(j, entry));

                if (existed != null)
                {
                    existed.AddLogEntries(entry.LogEntries);
                }
                else
                {
                    journal.Add(entry);
                }
            }
        }

        private static async Task<StepResult> ExecuteStepAsync(MessagingContext currentContext, IStep step)
        {
            Task<StepResult> executeAsync = step.ExecuteAsync(currentContext);
            if (executeAsync == null)
            {
                throw new InvalidOperationException(
                    $"Asynchronous result of step: {step.GetType().Name} returns 'null'");
            }

            StepResult nextResult = await executeAsync.ConfigureAwait(false);

            if (nextResult == null)
            {
                throw new InvalidOperationException(
                    $"Result of step: {step.GetType().Name} returns 'null'");
            }

            if (nextResult.MessagingContext == null)
            {
                throw new InvalidOperationException(
                    $"Result of step {step.GetType().Name} doesn't have a '{nameof(MessagingContext)}'");
            }

            return nextResult;
        }
    }
}
