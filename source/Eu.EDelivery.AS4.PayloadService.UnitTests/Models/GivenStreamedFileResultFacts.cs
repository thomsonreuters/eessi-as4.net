using System.IO;
using System.Text;
using Eu.EDelivery.AS4.PayloadService.Models;
using Xunit;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Models
{
    public class GivenStreamedFileResultFacts
    {
        [Fact]
        public void ReturnsExpectedResult()
        {
            const string expectedContent = "message data!";
            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent)))
            {
                var streamedFileResult = new StreamedFileResult(contentStream, "download-filename", "content-type");

                StreamedFileResultAssert.OnContent(
                    streamedFileResult: streamedFileResult, 
                    assertion: actualContent => Assert.Equal(expectedContent, actualContent));
            }
        }
    }
}
