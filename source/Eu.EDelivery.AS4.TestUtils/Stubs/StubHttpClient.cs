using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Moq;

namespace Eu.EDelivery.AS4.TestUtils.Stubs
{
    /// <summary>
    /// <see cref="IHttpClient" /> implementation to return a <see cref="AS4Message" />.
    /// </summary>
    public class StubHttpClient : IHttpClient
    {
        private readonly AS4Message _expectedMessage;
        private readonly HttpStatusCode _expectedStatusCode;

        private readonly System.Exception _exceptionToBeThrown;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubHttpClient" /> class.
        /// </summary>
        /// <param name="expectedMessage">The Expected <see cref="AS4Message" />.</param>
        /// <param name="expectedStatusCode">The expected status code.</param>
        private StubHttpClient(AS4Message expectedMessage, HttpStatusCode expectedStatusCode = HttpStatusCode.OK) : this(expectedStatusCode)
        {
            _expectedMessage = expectedMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StubHttpClient"/> class.
        /// </summary>
        /// <param name="expectedStatusCode">The expected status code.</param>
        private StubHttpClient(HttpStatusCode expectedStatusCode)
        {
            _expectedStatusCode = expectedStatusCode;
        }

        private StubHttpClient(System.Exception exception)
        {
            _exceptionToBeThrown = exception;
        }

        public bool IsCalled { get; private set; }

        /// <summary>
        /// Creates a <see cref="StubHttpClient"/> that returns an empty response with a given status code
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static StubHttpClient ThatReturns(HttpStatusCode statusCode) => new StubHttpClient(statusCode);

        /// <summary>
        /// Creates a <see cref="StubHttpClient"/> that returns a filled body with 
        /// </summary>
        /// <param name="as4Message"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static IHttpClient ThatReturns(AS4Message as4Message, HttpStatusCode statusCode = HttpStatusCode.OK) => new StubHttpClient(as4Message, statusCode);

        public static IHttpClient ThatThrows(System.Exception exception) => new StubHttpClient(exception);

        /// <summary>
        /// Request a Message for the <see cref="IHttpClient"/> implementation.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public HttpWebRequest Request(string url, string contentType)
        {
            var request = new Mock<HttpWebRequest>();

            request.Setup(r => r.GetRequestStreamAsync()).ReturnsAsync(Stream.Null);
            request.Setup(r => r.GetRequestStream()).Returns(Stream.Null);

            return request.Object;
        }

        /// <summary>
        /// Send a <see cref="HttpWebRequest" /> to the configured target.
        /// </summary>
        /// <param name="request">To be send <see cref="HttpWebRequest" />.</param>
        /// <returns></returns>
        public Task<(HttpWebResponse response, WebException exception)> Respond(HttpWebRequest request)
        {
            IsCalled = true;

            if (_exceptionToBeThrown != null)
            {
                throw _exceptionToBeThrown;
            }

            var response = new Mock<HttpWebResponse>();

            if (_expectedMessage != null)
            {
                response.Setup(r => r.GetResponseStream()).Returns(MessageStream(_expectedMessage));

            }
            response.Setup(r => r.StatusCode).Returns(_expectedStatusCode);
            response.Setup(r => r.ContentType).Returns(Constants.ContentTypes.Soap);

            return Task.FromResult((response.Object, (WebException) null));
        }

        private static Stream MessageStream(AS4Message message)
        {
            var messageStream = new MemoryStream();

            new SoapEnvelopeSerializer().Serialize(message, messageStream, CancellationToken.None);
            messageStream.Position = 0;

            return messageStream;
        }
    }
}