using System.Net;
using System.Threading;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send.Response
{
    /// <summary>
    /// Testing <see cref="AS4Response"/>
    /// </summary>
    public class GivenAS4ResponseFacts
    {
        [Fact]
        public void GetsInternalErrorStatus_IfInvalidHttpResponse()
        {
            Assert.Equal(HttpStatusCode.InternalServerError, CreateAS4ResponseWith(webResponse: null).StatusCode);
        }

        private static AS4Response CreateAS4ResponseWith(HttpWebResponse webResponse)
        {
            return new AS4Response(
                webResponse: webResponse, 
                resultedMessage: new NullInternalMessage(), 
                cancellation: CancellationToken.None);
        }
    }
}
