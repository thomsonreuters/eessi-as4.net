using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Retriever
{
    /// <summary>
    /// Testing <see cref="FilePayloadRetriever" />
    /// </summary>
    public class GivenFilePayloadRetrieverFacts
    {
        private readonly FilePayloadRetriever _retriever;

        public GivenFilePayloadRetrieverFacts()
        {
            _retriever = new FilePayloadRetriever();
        }
        
        /// <summary>
        /// Testing if the retriever fails
        /// </summary>
        public class GivenFilePayloadRetrieverFails : GivenFilePayloadRetrieverFacts
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