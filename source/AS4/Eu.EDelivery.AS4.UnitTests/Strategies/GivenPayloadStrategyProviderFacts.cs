using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies
{
    /// <summary>
    /// Testing the <see cref="PayloadRetrieverProvider" />
    /// </summary>
    public class GivenPayloadStrategyProviderFacts
    {
        private readonly PayloadRetrieverProvider _provider;

        public GivenPayloadStrategyProviderFacts()
        {
            _provider = new PayloadRetrieverProvider();
        }

        /// <summary>
        /// Testing the Provider with valid retriever
        /// </summary>
        public class GivenValidStrategy : GivenPayloadStrategyProviderFacts
        {
            [Fact]
            public void ThenProviderGetsStrategy()
            {
                // Arrange
                var mockedStrategy = new Mock<IPayloadRetriever>();

                // Act
                _provider.Accept(payload => true, mockedStrategy.Object);
                IPayloadRetriever result = _provider.Get(new Payload(string.Empty));

                // Assert
                Assert.NotNull(result);
                Assert.Equal(mockedStrategy.Object, result);
            }
        }

        /// <summary>
        /// Testing the Provider with invalid retriever
        /// </summary>
        public class GivenInvalidStrategy : GivenPayloadStrategyProviderFacts
        {
            [Fact]
            public void ThenProviderNotGetsStrategy()
            {
                // Arrange
                const string prefix = "file";
                var mockedStrategy = new Mock<IPayloadRetriever>();

                // Act / Assert
                _provider.Accept(payload => payload.Location.StartsWith(prefix), null);
                Assert.Throws<AS4Exception>(() => _provider.Get(new Payload(prefix)));
            }
        }
    }
}