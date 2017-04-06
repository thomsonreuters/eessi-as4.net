using System;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Xunit;
using System.Reflection;

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

        [Fact]
        public void GetsEmptyAS4MessageForEmptyHttpContentType()
        {
            var response = CreateWebResponse(string.Empty);

            Assert.Equal(string.Empty, response.ContentType);

            InternalMessage result = CreateAS4ResponseWith(webResponse: response).ResultedMessage;

            Assert.True(result.AS4Message.IsEmpty);
        }


        private static HttpWebResponse CreateWebResponse(string contentType)
        {
            // It is not possible to create a HttpWebResponse instance directly; therefore reflection is being used.
            var response = Activator.CreateInstance<HttpWebResponse>();

            FieldInfo fi = typeof(HttpWebResponse).GetField("m_HttpResponseHeaders", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fi == null)
            {
                throw new MissingFieldException("The m_HttpResponseHeaders field could not be retrieved from the HttpWebResponse class.");
            }

            fi.SetValue(response, new WebHeaderCollection(), BindingFlags.NonPublic, null, null);

            response.Headers.Add("Content-Type", contentType);

            return response;
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
