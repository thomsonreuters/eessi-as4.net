using System.IO;
using System.Net;
using System.Threading;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4Response" /> HTTP Web Response implementation.
    /// </summary>
    public class AS4Response : IAS4Response
    {
        private readonly HttpWebResponse _httpWebResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Response" /> class.
        /// </summary>
        /// <param name="webResponse">The web Response.</param>
        /// <param name="resultedMessage">The resulted Message.</param>
        /// <param name="cancellation">The cancellation.</param>
        public AS4Response(HttpWebResponse webResponse, InternalMessage resultedMessage, CancellationToken cancellation)
        {
            _httpWebResponse = webResponse;

            ResultedMessage = resultedMessage;
            Cancellation = cancellation;
        }

        /// <summary>
        /// Gets the Conten Type of the HTTP response.
        /// </summary>
        public string ContentType => _httpWebResponse?.ContentType;

        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        public HttpStatusCode StatusCode => _httpWebResponse?.StatusCode ?? HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets the Message from the AS4 response.
        /// </summary>
        public InternalMessage ResultedMessage { get; }

        /// <summary>
        /// Gets the cancellation information during the handling of the AS4 response.
        /// </summary>
        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Get the serialized stream of the HTTP response.
        /// </summary>
        /// <returns></returns>
        public Stream GetResponseStream()
        {
            return _httpWebResponse?.GetResponseStream();
        }
    }

    /// <summary>
    /// Contract to define the HTTP/AS4 response being handled.
    /// </summary>
    public interface IAS4Response
    {
        /// <summary>
        /// Gets the Conten Type of the HTTP response.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the Message from the AS4 response.
        /// </summary>
        InternalMessage ResultedMessage { get; }

        /// <summary>
        /// Gets the cancellation information during the handling of the AS4 response.
        /// </summary>
        CancellationToken Cancellation { get; }

        /// <summary>
        /// Get the serialized stream of the HTTP response.
        /// </summary>
        /// <returns></returns>
        Stream GetResponseStream();
    }
}