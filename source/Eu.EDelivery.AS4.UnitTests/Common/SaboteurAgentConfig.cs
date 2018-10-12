using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class SaboteurAgentConfig : PseudoConfig
    {
        private readonly Exception _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaboteurAgentConfig"/> class.
        /// </summary>
        public SaboteurAgentConfig() : this(exception: null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SaboteurAgentConfig"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public SaboteurAgentConfig(Exception exception)
        {
            _exception = exception;
        }

        /// <summary>
        /// Gets a value indicating whether if the Configuration is IsInitialized
        /// </summary>
        public override bool IsInitialized => true;

        /// <summary>
        /// Gets the agent settings.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AgentConfig> GetAgentsConfiguration()
        {
            if (_exception != null)
            {
                throw _exception;
            }

            return new AgentConfig[] {null};
        }
    }

    public class SaboteurAgentConfigFacts
    {
        [Fact]
        public void FailsToGetAgentConfiguration_WithException()
        {
            // Arrange
            var expectedException = new Exception();
            var sut = new SaboteurAgentConfig(expectedException);

            // Act / Assert
            Assert.Throws(expectedException.GetType(), () => sut.GetAgentsConfiguration());
        }

        [Fact]
        public void InvalidAgentConfiguration_WithoutException()
        {
            // Arrange
            var sut = new SaboteurAgentConfig();

            // Act / Assert
            Assert.All(sut.GetAgentsConfiguration(), Assert.Null);
        }
    }
}
