using System;
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
        private readonly Model.Internal.Steps _settingSteps;
        private readonly ConditionalStepConfig _conditialStepConfig;

        private StepBuilder(Model.Internal.Steps settingSteps, ConditionalStepConfig conditionalStepConfig)
        {
            _settingSteps = settingSteps;
            _conditialStepConfig = conditionalStepConfig;
        }

        /// <summary>
        /// Set the configured <see cref="Model.Internal.Steps"/> settings
        /// </summary>
        /// <param name="settingSteps"></param>
        /// <returns></returns>
        public static StepBuilder FromSettings(Model.Internal.Steps settingSteps)
        {
            return new StepBuilder(settingSteps, null);
        }

        public static StepBuilder FromConditionalConfig(ConditionalStepConfig conditionalStepConfig)
        {
            return new StepBuilder(null, conditionalStepConfig);
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
                    _conditialStepConfig.ThenStepConfig,
                    _conditialStepConfig.ElseStepConfig);

                return new[] {step};
            }

            return _settingSteps.Step.Select(CreateInstance);
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
                    _conditialStepConfig.ThenStepConfig,
                    _conditialStepConfig.ElseStepConfig);
            }

            IStep decoratedStep = CreateDecoratorStep(_settingSteps);
            IList<IStep> undecoratedSteps = CreateUndecoratedSteps();

            if (undecoratedSteps.Count == 0)
            {
                return decoratedStep;
            }

            undecoratedSteps.Insert(0, decoratedStep);
            return new CompositeStep(undecoratedSteps.ToArray());
        }

        private static IStep CreateDecoratorStep(Model.Internal.Steps settingsSteps)
        {
            IStep[] decoratedSteps = settingsSteps.Step
                .Where(s => s.UnDecorated == false)
                .Select(CreateInstance)
                .ToArray();

            var compositeStep = new CompositeStep(decoratedSteps);
            return settingsSteps.Decorator != null
                ? CreateInstance<IStep>(settingsSteps.Decorator, compositeStep)
                : compositeStep;
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

        private IList<IStep> CreateUndecoratedSteps()
        {
            return _settingSteps.Step
                .Where(s => s.UnDecorated)
                .Select(settingStep => CreateInstance<IStep>(settingStep.Type))
                .ToList();
        }

        private static T CreateInstance<T>(string typeString, params object[] args) where T : class
        {
            return GenericTypeBuilder.FromType(typeString).SetArgs(args).Build<T>();
        }
    }
}