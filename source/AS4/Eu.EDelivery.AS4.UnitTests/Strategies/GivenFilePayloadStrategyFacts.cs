using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies
{
    /// <summary>
    /// Testing <see cref="FilePayloadRetriever" />
    /// </summary>
    public class GivenFilePayloadStrategyFacts
    {
        private readonly FilePayloadRetriever _retriever;

        public GivenFilePayloadStrategyFacts()
        {
            _retriever = new FilePayloadRetriever();
        }

        /// <summary>
        /// Testing if the retriever fails
        /// </summary>
        public class GivenFilePayloadStrategyFails : GivenFilePayloadStrategyFacts
        {
            [Fact]
            public async Task ThenRetrievePayloadFails()
            {
                // Arrange
                const string location = "invalid-location";

                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(() => _retriever.RetrievePayloadAsync(location));
            }
        }
    }
}