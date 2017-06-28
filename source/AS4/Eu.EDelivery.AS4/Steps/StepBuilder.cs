using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Builder to make <see cref="IStep"/> implementation
    /// from <see cref="Step"/> settings
    /// </summary>
    public class StepBuilder
    {
        private readonly Step[] _stepConfiguration;
        private readonly ConditionalStepConfig _conditialStepConfig;

        private StepBuilder(Step[] stepConfiguration, ConditionalStepConfig conditionalStepConfig)
        {
            _stepConfiguration = stepConfiguration;
            _conditialStepConfig = conditionalStepConfig;
        }

        /// <summary>
        /// Set the configured <see cref="Step"/> settings
        /// </summary>
        /// <param name="settingSteps"></param>
        /// <returns></returns>
        public static StepBuilder FromSettings(Step[] settingSteps)
        {
            return new StepBuilder(settingSteps, null);
        }

        /// <summary>
        /// Set the configured <see cref="Step"/> settings.
        /// </summary>
        /// <param name="conditionalStepConfig">The conditional step configuration.</param>
        /// <returns></returns>
        public static StepBuilder FromConditionalConfig(ConditionalStepConfig conditionalStepConfig)
        {
            return new StepBuilder(stepConfiguration: null, conditionalStepConfig: conditionalStepConfig);
        }

        /// <summary>
        /// Build the <see cref="IStep"/> implementation
        /// </summary>
        /// <returns></returns>
        public IStep Build()
        {
            if (_conditialStepConfig != null)
            {
                return new ConditionalStep(
                    _conditialStepConfig.Condition,
                    _conditialStepConfig.ThenSteps,
                    _conditialStepConfig.ElseSteps);
            }

            IStep[] steps = _stepConfiguration.Select(CreateInstance).ToArray();
            return new CompositeStep(steps);
        }

        /// <summary>
        /// Builds the steps.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IStep> BuildSteps()
        {
            if (_conditialStepConfig != null)
            {
                var step = new ConditionalStep(
                    _conditialStepConfig.Condition,
                    _conditialStepConfig.ThenSteps,
                    _conditialStepConfig.ElseSteps);

                return new[] {step};
            }

            return _stepConfiguration.Select(CreateInstance);
        }

        private static IStep CreateInstance(Step settingStep)
        {
            return settingStep.Setting != null
                ? CreateConfigurableStep(settingStep)
                : CreateInstance<IStep>(settingStep.Type);
        }

        private static IConfigStep CreateConfigurableStep(Step settingStep)
        {
            var step = CreateInstance<IConfigStep>(settingStep.Type);

            Dictionary<string, string> dictionary = settingStep.Setting
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            step.Configure(dictionary);

            return step;
        }

        private static T CreateInstance<T>(string typeString, params object[] args) where T : class
        {
            return GenericTypeBuilder.FromType(typeString).SetArgs(args).Build<T>();
        }
    }
}