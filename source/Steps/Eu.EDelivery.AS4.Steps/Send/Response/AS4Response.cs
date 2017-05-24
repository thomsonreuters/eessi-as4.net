using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4Response" /> HTTP Web Response implementation.
    /// </summary>
    internal class AS4Response : IAS4Response
    {
        private readonly HttpWebResponse _httpWebResponse;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Response" /> class.
        /// </summary>
        /// <param name="requestMessage">The resulted Message.</param>
        /// <param name="webResponse">The web Response.</param>
        private AS4Response(InternalMessage requestMessage, HttpWebResponse webResponse)
        {
            _httpWebResponse = webResponse;

            OriginalRequest = requestMessage;
        }

        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        public HttpStatusCode StatusCode => _httpWebResponse?.StatusCode ?? HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets the Message from the AS4 response.
        /// </summary>
        public InternalMessage ResultedMessage { get; private set; }

        /// <summary>
        /// Gets the Original Request from this response.
        /// </summary>
        public InternalMessage OriginalRequest { get; }

        /// <summary>
        /// Create a new <see cref="AS4Response"/> instance.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="webResponse"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<AS4Response> Create(InternalMessage requestMessage, HttpWebResponse webResponse, CancellationToken cancellation)
        {
            var response = new AS4Response(requestMessage, webResponse)
            {
                ResultedMessage = await TryDeserializeHttpResponse(webResponse, cancellation).ConfigureAwait(false)
            };

            response.ResultedMessage.SendingPMode = response.OriginalRequest?.SendingPMode;

            return response;
        }

        private static async Task<InternalMessage> TryDeserializeHttpResponse(WebResponse webResponse, CancellationToken cancellation)
        {
            AS4Message deserializedResponse;

            try
            {
                if (string.IsNullOrWhiteSpace(webResponse.ContentType))
                {
                    Logger.Info("No ContentType set - returning an empty AS4 response.");
                    return new InternalMessage(new AS4MessageBuilder().Build());
                }

                ISerializer serializer = Registry.Instance.SerializerProvider.Get(webResponse.ContentType);

                deserializedResponse = await serializer
                    .DeserializeAsync(webResponse.GetResponseStream(), webResponse.ContentType, cancellation).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                deserializedResponse = new AS4MessageBuilder().Build();
            }

            return new InternalMessage(deserializedResponse);
        }
    }

    /// <summary>
    /// Contract to define the HTTP/AS4 response being handled.
    /// </summary>
    public interface IAS4Response
    {
        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the Message from the AS4 response.
        /// </summary>
        InternalMessage ResultedMessage { get; }

        /// <summary>
        /// Gets the Original Request from this response.
        /// </summary>
        InternalMessage OriginalRequest { get; }
    }
}