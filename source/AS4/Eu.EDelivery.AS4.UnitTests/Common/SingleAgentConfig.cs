using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Receivers;
using Eu.EDelivery.AS4.UnitTests.Steps;
using Eu.EDelivery.AS4.UnitTests.Transformers;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class SingleAgentConfig : PseudoConfig
    {
        private static Transformer TransformerConfig { get; } = new Transformer
        {
            Type = typeof(DummyTransformer).AssemblyQualifiedName
        };

        private static Step[] ExpectedStep { get; } = { new Step { Type = typeof(DummyStep).AssemblyQualifiedName } };

        /// <summary>
        /// Gets a value indicating whether if the Configuration is IsInitialized
        /// </summary>
        public override bool IsInitialized => true;

        /// <summary>
        /// Gets the retention period (in days) for which the stored entities are cleaned-up.
        /// </summary>
        /// <value>The retention period in days.</value>
        public override TimeSpan RetentionPeriod => default(TimeSpan);

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AgentConfig> GetAgentsConfiguration()
        {
            var settings =
                new AgentSettings
                {
                    Receiver = new Receiver { Type = typeof(StubReceiver).AssemblyQualifiedName },
                    Transformer = TransformerConfig,
                    StepConfiguration = new StepConfiguration
                    {
                        NormalPipeline = ExpectedStep,
                        ErrorPipeline = ExpectedStep
                    }
                };

            yield return new AgentConfig(null) { Settings = settings };
        }

        /// <summary>
        /// Gets the configuration of the Minder Test-Agents that are enabled.
        /// </summary>
        /// <returns></returns>
        /// <remarks>For every SettingsMinderAgent that is returned, a special Minder-Agent will be instantiated.</remarks>
        public override IEnumerable<SettingsMinderAgent> GetEnabledMinderTestAgents()
        {
            return Enumerable.Empty<SettingsMinderAgent>();
        }
    }

}