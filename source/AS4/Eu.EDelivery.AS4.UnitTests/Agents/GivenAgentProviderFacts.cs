using System;
using System.Collections.Generic;
    using System.Linq;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ServiceHandler.Agents;
using Eu.EDelivery.AS4.UnitTests.Common;
using FsCheck;
using FsCheck.Xunit;
using Moq;
using Newtonsoft.Json;
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
            var sut = new AgentProvider(new SaboteurAgentConfig(expectedException), Mock.Of<IRegistry>());

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.Empty(agents);
        }

        [Fact]
        public void AssembleAgentBaseClasses_IfTypeIsSpecified()
        {
            // Arrange
            // Minder agents are being created and uses the registry
            Registry.Instance.Initialize(StubConfig.Default);
            var sut = new AgentProvider(new SingleAgentConfig(), Mock.Of<IRegistry>());

            // Act
            IEnumerable<IAgent> agents = sut.GetAgents();

            // Assert
            Assert.NotEmpty(agents);
        }

        [Property]
        public Property Default_Transformers_Are_Serializable(AgentType type)
        {
            // Arrange
            TransformerConfigEntry expected = AgentProvider.GetDefaultTransformerForAgentType(type);
            string json = JsonConvert.SerializeObject(expected);

            // Act
            var actual = JsonConvert.DeserializeObject<TransformerConfigEntry>(json);

            // Assert
            bool sameDefault = expected.DefaultTransformer.Type == actual.DefaultTransformer.Type;
            bool sameOthers = expected.OtherTransformers
                .Zip(actual.OtherTransformers, (t1, t2) => t1.Type == t2.Type)
                .All(x => x);

            return sameDefault.ToProperty().And(sameOthers);
        }
    }
}
