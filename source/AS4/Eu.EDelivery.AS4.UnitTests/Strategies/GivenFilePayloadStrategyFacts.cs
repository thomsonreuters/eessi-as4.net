using System;
using System.IO;
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
            this._retriever = new FilePayloadRetriever();
        }

        /// <summary>
        /// Testing if the retriever fails
        /// </summary>
        public class GivenFilePayloadStrategyFails
            : GivenFilePayloadStrategyFacts
        {
            [Fact]
            public void ThenRetrievePayloadFails()
            {
                // Arrange
                const string location = "invalid-location";
                // Act / Assert
                Assert.Throws<AS4Exception>(() => base._retriever.RetrievePayload(location));
            }
        }
    }
}