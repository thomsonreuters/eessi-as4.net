using System.IO;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Repositories
{
    public class GivenAS4MessageBodyFileRetrieverFacts
    {
        [Theory]
        [InlineData(null)]
        [InlineData("file:///not-existing-path")]
        public void RetrieverReturnsNullStreams_IfFileDoesntExists(string stubLocation)
        {
            // Arrange
            var sut = new AS4MessageBodyFileRetriever();

            // Act
            Stream actualStream = sut.LoadAS4MessageStream(stubLocation);

            // Assert
            Assert.Null(actualStream);
        }        
    }
}
