using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.PayloadService.Infrastructure;
using Eu.EDelivery.AS4.PayloadService.Models;
using Xunit;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Infrastructure
{
    /// <summary>
    /// Testing <see cref="MultipartPayloadReader"/>
    /// </summary>
    public class GivenMultipartPayloadReaderFacts
    {
        [Fact]
        public void CannotCreateReader_IfContentTypeIsntMultiPart()
        {
            Assert.False(MultipartPayloadReader.TryCreate(Stream.Null, "application/json").Item1);
        }

        [Fact]
        public async Task ReadsExpectedContent()
        {
            // Arrange
            const string expectedContent = "message data!";
            using (var actualStream = new MemoryStream())
            {
                MultipartPayloadReader reader = await CreateStubReaderThatReturns(expectedContent, actualStream);
                var waitHandle = new ManualResetEvent(initialState: false);

                // Act
                await reader.StartReading(payload =>
                {
                    // Assert
                    Assert.Equal(expectedContent, ActualContentOf(payload));

                    waitHandle.Set();
                    return Task.CompletedTask;
                });

                Assert.True(waitHandle.WaitOne(timeout: TimeSpan.FromSeconds(1)));
            }
        }

        private static string ActualContentOf(Payload payload)
        {
            using (var streamReader = new StreamReader(payload.Content))
            {
                return streamReader.ReadToEnd();
            }
        }

        private static async Task<MultipartPayloadReader> CreateStubReaderThatReturns(string expectedContent, Stream actualStream)
        {
            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedContent)))
            {
                var multipartContent = new MultipartFormDataContent {{new StreamContent(contentStream), "name", "filename"}};
                await multipartContent.CopyToAsync(actualStream);
                actualStream.Position = 0;

                return MultipartPayloadReader.TryCreate(
                    actualStream, multipartContent.Headers.ContentType.ToString()).Item2;
            }
        }
    }
}