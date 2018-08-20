using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Retriever
{
    public class GivenTempFilePayloadRetrieverFacts
    {
        [Fact]
        public async Task TemporaryFileGetsDeletedAfterBeingRetrieved()
        {
            // Arrange
            string fixture = Path.GetTempFileName();
            var sut = new TempFilePayloadRetriever();

            // Act
            (await sut.RetrievePayloadAsync(fixture)).Dispose();

            // Assert
            Assert.False(File.Exists(fixture), "Temporary file isn't deleted afterwards");
        }
    }
}
