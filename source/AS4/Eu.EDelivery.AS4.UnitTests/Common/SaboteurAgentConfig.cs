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
        /// Gets the settings agents.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<AgentSettings> GetSettingsAgents()
        {
            if (_exception != null)
            {
                throw _exception;
            }

            return new AgentSettings[] {null};
        }
    }

    public class SaboteurAgentConfigFacts
    {
        [Fact]
        public void ThrowsExpectedException_IfDefined()
        {
            // Arrange
            var expectedException = new Exception();
            var sut = new SaboteurAgentConfig(expectedException);

            // Act / Assert
            Assert.Throws(expectedException.GetType(), () => sut.GetSettingsAgents());
        }

        [Fact]
        public void ReturnsInvalid_IfNotDefined()
        {
            // Arrange
            var sut = new SaboteurAgentConfig();

            // Act
            IEnumerable<AgentSettings> agents = sut.GetSettingsAgents();

            // Assert
            Assert.Collection(agents, Assert.Null);
        }
    }
}
