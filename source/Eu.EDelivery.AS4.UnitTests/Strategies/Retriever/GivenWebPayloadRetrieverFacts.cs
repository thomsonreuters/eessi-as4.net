using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Retriever
{
    public class GivenWebPayloadRetrieverFacts
    {
        [Fact]
        public async Task ThenDownloadPayloadSucceeds()
        {
            // Arrange
            const string expectedPayload = "message data!", location = "http://ignored/path";
            var retriever = new HttpPayloadRetriever(request => RespondWithContent(expectedPayload));

            // Act
            using (var streamReader = new StreamReader(await retriever.RetrievePayloadAsync(location)))
            {
                // Assert
                string actualPayload = streamReader.ReadToEnd();
                Assert.Equal(expectedPayload, actualPayload);
            }
        }

        private static Task<HttpResponseMessage> RespondWithContent(string expectedPayload)
        {
            var response = new HttpResponseMessage {Content = new StringContent(expectedPayload)};
            return Task.FromResult(response);
        }

        [Fact]
        public async Task ThenDownloadFailed_IfReturnCodeIsntSuccessful()
        {
            // Arrange
            const string location = "http://ignored/path";
            var retriever = new HttpPayloadRetriever(request => RespondWithStatusCode(HttpStatusCode.BadGateway));

            // Act
            Stream actualPayload = await retriever.RetrievePayloadAsync(location);

            // Assert
            Assert.Equal(Stream.Null, actualPayload);
        }

        private static Task<HttpResponseMessage> RespondWithStatusCode(HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage(statusCode);
            return Task.FromResult(response);
        }
    }
}