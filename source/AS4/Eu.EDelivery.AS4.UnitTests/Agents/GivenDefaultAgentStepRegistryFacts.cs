using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class GivenDefaultAgentStepRegistryFacts
    {
        [Fact]
        public void RegistryContainsDefaultConfigurationForAllAgentTypes()
        {
            var agentTypes = (AgentType[])Enum.GetValues(typeof(AgentType));

            foreach (var agentType in agentTypes)
            {
                var config = AgentProvider.GetDefaultStepConfigurationForAgentType(agentType);

                Assert.NotNull(config);
            }
        }
    }
}
