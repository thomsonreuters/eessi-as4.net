using System.IO;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    /// <summary>
    /// <see cref="IDeliverSender"/> implementation to deliver via HTTP.
    /// </summary>
    public class HttpDeliverySender : IDeliverSender
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private Method _method;

        /// <summary>
        /// Configure the <see cref="IDeliverSender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _method = method;
        }

        /// <summary>
        /// Send a given <paramref name="message"/> to a specified endpoint
        /// </summary>
        /// <param name="message">The message.</param>
        public async void Send(DeliverMessageEnvelope message)
        {
            string destinationUri = _method["location"].Value;
            HttpWebRequest request = CreateDeliverRequest(message, destinationUri);
            HttpWebResponse response = await SendDeliverRequest(request);

            response?.Close();
        }

        private HttpWebRequest CreateDeliverRequest(DeliverMessageEnvelope deliverMessage, string destinationUri)
        {
            // TODO: verify if destinationUri is a valid http endpoint.                        
            HttpWebRequest request = HttpPostRequest.Create(destinationUri, deliverMessage.ContentType);

            _log.Info($"Send Notification {deliverMessage.MessageInfo.MessageId} to {destinationUri}");

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(deliverMessage.DeliverMessage, 0, deliverMessage.DeliverMessage.Length);
            }

            return request;
        }

        private async Task<HttpWebResponse> SendDeliverRequest(WebRequest request)
        {
            HttpWebResponse response;

            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;

                if (response == null)
                {
                    _log.Error("No WebResponse received for http delivery.");
                }
            }
            catch (WebException exception)
            {
                response = exception.Response as HttpWebResponse;
            }

            bool isInvalidResponse = response != null && !IsResponseValid(response);
            if (isInvalidResponse)
            {
                LogErrorFrom(response);
            }

            return response;
        }

        private static bool IsResponseValid(HttpWebResponse response)
        {
            return 
                response.StatusCode == HttpStatusCode.Accepted || 
                response.StatusCode == HttpStatusCode.OK;
        }

        private void LogErrorFrom(HttpWebResponse response)
        {
            _log.Error($"Unexpected response received for http notification: {response.StatusCode}");
            Stream responseStream = response.GetResponseStream();

            if (!_log.IsErrorEnabled || responseStream == null)
            {
                return;
            }

            using (var streamReader = new StreamReader(responseStream, detectEncodingFromByteOrderMarks: true))
            {
                _log.Error(streamReader.ReadToEnd());
            }
        }
    }
}