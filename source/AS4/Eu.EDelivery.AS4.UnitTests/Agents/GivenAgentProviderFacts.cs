using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Agents;
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
            var sut = new AgentProvider(new SingleAgentBaseConfig());

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.All(agents, a => Assert.IsType<AgentBase>(a));
        }
    }
}
