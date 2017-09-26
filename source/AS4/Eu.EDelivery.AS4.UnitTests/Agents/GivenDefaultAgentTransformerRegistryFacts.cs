using System;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class GivenDefaultAgentTransformerRegistryFacts
    {
        [Fact]
        public void RegistryContainsDefaultTransformerForAllAgentTypes()
        {
            var agentTypes = (AgentType[])Enum.GetValues(typeof(AgentType));

            foreach (var agentType in agentTypes)
            {                
                var transformer = AgentProvider.GetDefaultTransformerForAgentType(agentType);

                Assert.NotNull(transformer);
            }
        }
    }
}