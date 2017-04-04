using System.Net;
using System.Threading;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send.Response
{
    /// <summary>
    /// Testing <see cref="AS4Response"/>
    /// </summary>
    public class GivenAS4ResponseFacts
    {
        [Fact]
        public void GetsRequestMessageFromAS4Response()
        {
            // Arranage
            var expectedRequest = new InternalMessage();

            // Act
            InternalMessage actualRequest = CreateAS4ResponseWith(messageRequest: expectedRequest).OriginalRequest;

            // Assert
            Assert.Equal(expectedRequest, actualRequest);
        }

        [Fact]
        public void GetsInternalErrorStatus_IfInvalidHttpResponse()
        {
            Assert.Equal(HttpStatusCode.InternalServerError, CreateAS4ResponseWith(webResponse: null).StatusCode);
        }

        private static AS4Response CreateAS4ResponseWith(HttpWebResponse webResponse = null, InternalMessage messageRequest = null)
        {
            return AS4Response.Create(
                requestMessage: messageRequest, 
                webResponse: webResponse, 
                cancellation: CancellationToken.None).Result;
        }
    }
}
