using System;
using System.Collections;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    /// <summary>
    /// Testing <see cref="Agent"/>
    /// </summary>
    public class GivenAgentFacts
    {
        public class Constructor : IEnumerable<object[]>
        {
            [Theory]
            [ClassData(typeof(Constructor))]
            public void ThrowArgumentNullException(
                AgentConfig agentConfig,
                IReceiver receiver,
                Transformer transformerConfig,
                AS4.Model.Internal.Steps stepConfiguration)
            {
                Assert.Throws<ArgumentNullException>(
                    () => new Agent(agentConfig, receiver, transformerConfig, stepConfiguration));
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<object[]> GetEnumerator()
            {
                IReceiver receiver = new Mock<IReceiver>().Object;
                var agentConfig = new AgentConfig(name: "ignored name");
                var transformerConfig = new Transformer();
                var stepConfiguration = new AS4.Model.Internal.Steps();

                yield return new object[] {null, receiver, transformerConfig, stepConfiguration};
                yield return new object[] {agentConfig, null, transformerConfig, stepConfiguration};
                yield return new object[] {agentConfig, receiver, null, stepConfiguration};
                yield return new object[] {agentConfig, receiver, transformerConfig, null};
            }
        }
    }
}