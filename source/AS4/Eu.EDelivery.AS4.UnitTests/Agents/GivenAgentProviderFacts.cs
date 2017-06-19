using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Receivers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class GivenAgentProviderFacts
    {
        [Fact]
        public void DontCatchesWrongAgentConfig()
        {
            // Act / Assert
            Assert.ThrowsAny<Exception>(() => new AgentProvider(new SaboteurAgentConfig()));
        }

        [Fact]
        public void CatchesAS4Exceptions()
        {
            // Arrange
            var expectedException = new AS4Exception("ignored string");
            var sut = new AgentProvider(new SaboteurAgentConfig(expectedException));

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.Empty(agents);
        }

        [Fact]
        public void AssemblesAgents()
        {
            // Arrange
            var sut = new AgentProvider(new SingleAgentConfig());

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.Equal(1, agents.Count());
            IAgent singleAgent = agents.First();

            Assert.Equal(SingleAgentConfig.ExpectedSteps, GetsActualSteps(singleAgent));
            Assert.Equal(SingleAgentConfig.TransformerConfig, GetsActualTransformer(singleAgent));
            Assert.IsType<StubReceiver>(GetsActualReceiver(singleAgent));
        }

        private static AS4.Model.Internal.Steps GetsActualSteps(IAgent agent)
        {
            return GetField<AS4.Model.Internal.Steps>(agent, "_stepConfiguration");
        }

        private static Transformer GetsActualTransformer(IAgent agent)
        {
            return GetField<Transformer>(agent, "_transformerConfiguration");
        }

        private static IReceiver GetsActualReceiver(IAgent agent)
        {
            return GetField<IReceiver>(agent, "_receiver");
        }

        private static T GetField<T>(IAgent agent, string field) where T : class
        {
            FieldInfo stepConfig = agent.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(stepConfig);

            return stepConfig.GetValue(agent) as T;
        }

        [Fact]
        public void AssembleAgentBaseClasses_IfTypeIsSpecified()
        {
            // Arrange
            var sut = new AgentProvider(new SingleAgentBaseConfig());

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.All(agents, a => Assert.IsType<AgentBase>(a));
        }
    }
}
