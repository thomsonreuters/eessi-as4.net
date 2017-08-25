using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class GivenAgentProviderFacts
    {
        [Fact]
        public void CatchesExceptionsWhenBuildingAgents()
        {
            // Arrange
            var expectedException = new Exception("ignored string");
            var sut = new AgentProvider(new SaboteurAgentConfig(expectedException));

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.Empty(agents);
        }

        [Fact]
        public void AssembleAgentBaseClasses_IfTypeIsSpecified()
        {
            // Arrange
            var sut = new AgentProvider(new SingleAgentConfig());

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.All(agents, a => Assert.IsType<Agent>(a));
        }
    }
}
