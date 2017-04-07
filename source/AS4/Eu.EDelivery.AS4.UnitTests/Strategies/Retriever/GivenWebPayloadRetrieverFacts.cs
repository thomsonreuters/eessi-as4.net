using System.IO;
using System.Net;
using System.Net.Http;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.UnitTests.Http;
using SimpleHttpMock;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Retriever
{
    public class GivenWebPayloadRetrieverFacts
    {
        private static readonly string SharedUrl = UniqueHost.Create().Url;

        [Fact]
        public void ThenDownloadPayloadSucceeds()
        {
            const string expectedPayload = "message data!";
            var retriever = new HttpPayloadRetriever();

            using (CreateStubServerThatReturns(expectedPayload))
            using (var streamReader = new StreamReader(retriever.RetrievePayload(SharedUrl)))
            {
                string actualPayload = streamReader.ReadToEnd();
                Assert.Equal(expectedPayload, actualPayload);
            }
        }

        [Fact]
        public void ThenDownloadFailed_IfReturnCodeIsntSuccessful()
        {
            var retriever = new HttpPayloadRetriever();

            using (CreateStubServerThatReturns(content: null, statusCode: HttpStatusCode.BadGateway))
            {
                Assert.Equal(Stream.Null, retriever.RetrievePayload(SharedUrl));
            }
        }

        private static MockedHttpServer CreateStubServerThatReturns(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var builder = new MockedHttpServerBuilder();

            builder.WhenGet(SharedUrl).RespondContent(statusCode, request => new StringContent(content));

            return builder.Build(SharedUrl);
        }
    }
}