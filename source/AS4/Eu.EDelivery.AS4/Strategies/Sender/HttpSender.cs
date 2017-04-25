using System.IO;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// <see cref="IDeliverSender"/>, <see cref="INotifySender"/> implemetation to HTTP POST on a configured endpoint.
    /// </summary>
    [Info("HTTP")]
    public class HttpSender : IDeliverSender, INotifySender
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IHttpClient _httpClient;
        private string _destinationUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSender"/> class.
        /// </summary>
        public HttpSender() : this(new ReliableHttpClient()) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSender"/> class.
        /// </summary>
        /// <param name="client">HTTP client to handle the request/respond actions.</param>
        public HttpSender(IHttpClient client)
        {
            _httpClient = client;
        }

        /// <summary>
        /// Configure the <see cref="IDeliverSender" />
        /// with a given <paramref name="method" />
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _destinationUri = method["location"].Value;
        }

        /// <summary>
        /// Start sending the <see cref="DeliverMessage" />
        /// </summary>
        /// <param name="deliverMessage"></param>
        public async Task SendAsync(DeliverMessageEnvelope deliverMessage)
        {
            Logger.Info($"Send Deliver {deliverMessage.MessageInfo.MessageId} to {_destinationUri}");

            HttpWebRequest request = CreateHttpPostRequest(deliverMessage.ContentType, deliverMessage.DeliverMessage);
            HttpWebResponse response = await SendHttpPostRequest(request);

            response?.Close();
        }

        /// <summary>
        /// Start sending the <see cref="NotifyMessage" />
        /// </summary>
        /// <param name="notifyMessage"></param>
        public async Task SendAsync(NotifyMessageEnvelope notifyMessage)
        {
            Logger.Info($"Send Notification {notifyMessage.MessageInfo.MessageId} to {_destinationUri}");

            HttpWebRequest request = CreateHttpPostRequest(notifyMessage.ContentType, notifyMessage.NotifyMessage);
            HttpWebResponse httpPostResponse = await SendHttpPostRequest(request);

            httpPostResponse?.Close();
        }

        private HttpWebRequest CreateHttpPostRequest(string contentType, byte[] contents)
        {
            // TODO: verify if destinationUri is a valid http endpoint.
            HttpWebRequest request = _httpClient.Request(_destinationUri, contentType);

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(contents, 0, contents.Length);
            }

            return request;
        }

        private async Task<HttpWebResponse> SendHttpPostRequest(HttpWebRequest request)
        {
            HttpWebResponse response = (await _httpClient.Respond(request)).response;

            if (response == null)
            {
                Logger.Error("No WebResponse received for http notification.");
            }

            bool isInvalidResponse = response != null && !IsResponseValid(response);
            if (isInvalidResponse)
            {
                LogErrorResponse(response);
            }

            return response;
        }

        private static void LogErrorResponse(HttpWebResponse response)
        {
            Logger.Error($"Unexpected response received for http notification: {response.StatusCode}");
            Stream responseStream = response.GetResponseStream();

            if (!Logger.IsErrorEnabled || responseStream == null)
            {
                return;
            }

            using (var streamReader = new StreamReader(responseStream, detectEncodingFromByteOrderMarks: true))
            {
                Logger.Error(streamReader.ReadToEnd());
            }
        }

        private static bool IsResponseValid(HttpWebResponse response)
        {
            return
                response.StatusCode == HttpStatusCode.Accepted ||
                response.StatusCode == HttpStatusCode.OK;
        }
    }
}