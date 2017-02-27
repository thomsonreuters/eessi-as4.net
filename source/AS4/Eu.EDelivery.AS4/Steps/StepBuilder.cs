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
            this._settingSteps = settingSteps;
            this._conditialStepConfig = conditionalStepConfig;
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
        /// Build the <see cref="IStep"/> implementation
        /// </summary>
        /// <returns></returns>
        public IStep Build()
        {
            if (this._conditialStepConfig != null)
            {
                return new ConditionalStep(_conditialStepConfig.Condition,
                    this._conditialStepConfig.ThenStepConfig,
                    this._conditialStepConfig.ElseStepConfig);
            }

            IStep decoratedStep = CreateDecoratorStep(this._settingSteps);
            IList<IStep> unDecoratedSteps = CreateUndecoratedSteps();

            if (unDecoratedSteps.Count == 0)
                return decoratedStep;

            unDecoratedSteps.Insert(0, decoratedStep);
            return new CompositeStep(unDecoratedSteps.ToArray());
        }


        private IStep CreateDecoratorStep(Model.Internal.Steps settingsSteps)
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

        private IStep CreateInstance(Step settingStep)
        {
            return settingStep.Setting != null
                ? CreateConfigurableStep(settingStep)
                : CreateInstance<IStep>(settingStep.Type);
        }

        private IConfigStep CreateConfigurableStep(Step settingStep)
        {
            var step = CreateInstance<IConfigStep>(settingStep.Type);

            Dictionary<string, string> dictionary = settingStep.Setting
                .ToDictionary(setting => setting.Key, setting => setting.Value);

            step.Configure(dictionary);

            return step;
        }

        private IList<IStep> CreateUndecoratedSteps()
        {
            return this._settingSteps.Step
                .Where(s => s.UnDecorated == true)
                .Select(settingStep => CreateInstance<IStep>(settingStep.Type))
                .ToList();
        }

        private static T CreateInstance<T>(string typeString, params object[] args) where T : class
        {
            return new GenericTypeBuilder().SetType(typeString).SetArgs(args).Build<T>();
        }
    }

    [Flags]
    public enum StepOptions
    {
        UseDatastore = 2,
        UseDefaults = 1
    }

    public class StepEntry
    {
        public IStep Step { get; set; }
        public StepOptions Options { get; set; }

        private StepEntry(IStep step, StepOptions options)
        {
            this.Step = step;
            this.Options = options;
        }

        public static StepEntry Create(StepOptions options, IStep step)
        {
            return new StepEntry(step, options);
        }
    }
}