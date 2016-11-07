using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// </summary>
    public class GivenRegistryFacts
    {
        private readonly IRegistry _registry;

        public GivenRegistryFacts()
        {
            this._registry = new Registry();
        }

        /// <summary>
        /// Testing the Registry with valid arguments
        /// </summary>
        public class GivenValidArgumentsRegistry : GivenRegistryFacts
        {
            [Fact]
            public void ThenGetFilePayloadStrategyProvider()
            {
                // Act
                IPayloadRetrieverProvider provider = base._registry.PayloadRetrieverProvider;
                // Assert
                IPayloadRetriever fileRetriever = provider.Get(new Payload(location: "file:///"));
                Assert.NotNull(fileRetriever);
            }

            [Fact]
            public void ThenGetWebPayloadStrategyProvider()
            {
                // Act
                IPayloadRetrieverProvider provider = base._registry.PayloadRetrieverProvider;
                // Assert
                IPayloadRetriever webRetriever = provider.Get(new Payload(location:"http"));
                Assert.NotNull(webRetriever);
            }
        }

        /// <summary>
        /// Testing the Registry with invalid arguments
        /// </summary>
        public class GivenInvalidArgumentsRegistry : GivenRegistryFacts
        {
            [Fact]
            public void ThenProvidersDoesNotHasPayloadStrategy()
            {
                // Act
                IPayloadRetrieverProvider provider = base._registry.PayloadRetrieverProvider;
                // Assert
                Assert.Throws<AS4Exception>(() 
                    => provider.Get(new Payload(location:"not-supported-location")));
            }
        }
    }
}