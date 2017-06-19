using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class GivenConfigFacts
    {
        [Fact]
        public void GetsExcpetedAgentsConfiguration()
        {
            // Arrange
            Config.Instance.Initialize();

            // Act
            IEnumerable<AgentConfig> configs = Config.Instance.GetAgentsConfiguration();

            // Assert
            Assert.NotEmpty(configs);
        }
    }
}
