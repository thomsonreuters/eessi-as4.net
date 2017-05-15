using System.IO;
using Eu.EDelivery.AS4.Streaming;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Streaming
{
    /// <summary>
    /// <see cref="NonCloseableStream"/>
    /// </summary>
    public class GivenNonClosableStreamFacts
    {
        [Fact]
        public void OuterStreamDoesntCloseInnerStream()
        {
            // Arrange
            using (var innerStream = new MemoryStream())
            using (var sut = new NonCloseableStream(innerStream))
            {
                // Act
                sut.Close();

                // Assert
                Assert.True(sut.CanRead);
                Assert.True(sut.CanWrite);
            }
        }
    }
}
