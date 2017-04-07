using System.IO;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Deliver;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public class HttpDeliverySender : DeliverySender
    {
        /// <summary>
        /// Send a given <paramref name="deliverMessage"/> to a specified <paramref name="destinationUri"/>.
        /// </summary>
        /// <param name="deliverMessage">The message.</param>
        /// <param name="destinationUri">The uri.</param>
        protected override void SendDeliverMessage(DeliverMessageEnvelope deliverMessage, string destinationUri)
        {
            // TODO: verify if destinationUri is a valid http endpoint.                        
            HttpWebRequest request = CreateWebRequest(destinationUri, deliverMessage.ContentType);

            Log.Info($"Send Notification {deliverMessage.MessageInfo.MessageId} to {destinationUri}");

            WriteDeliverMessageToRequest(deliverMessage, request);
            HandleDeliverResponse(request);
        }

        private static HttpWebRequest CreateWebRequest(string targetUrl, string contentType)
        {
            var request = (HttpWebRequest) WebRequest.Create(targetUrl);
            request.Method = "POST";
            request.ContentType = contentType;
            request.KeepAlive = false;
            request.Connection = "Open";
            request.ProtocolVersion = HttpVersion.Version11;

            ServicePointManager.Expect100Continue = false;

            return request;
        }

        private static void WriteDeliverMessageToRequest(DeliverMessageEnvelope deliverMessage, HttpWebRequest request)
        {
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(deliverMessage.DeliverMessage, 0, deliverMessage.DeliverMessage.Length);
            }
        }

        private async void HandleDeliverResponse(HttpWebRequest request)
        {
            HttpWebResponse response = null;

            try
            {
                response = await TrySendRequest(request);

                if (response == null)
                {
                    Log.Error("No WebResponse received for http delivery.");
                    return;
                }

                if (!IsResponseValid(response))
                {
                    Log.Error($"Unexpected response received for http delivery: {response.StatusCode}");

                    if (Log.IsErrorEnabled)
                    {
                        LogErrorFrom(response);
                    }
                }
            }
            finally
            {
                response?.Close();
            }
        }

        private async Task<HttpWebResponse> TrySendRequest(HttpWebRequest request)
        {
            try
            {
                return await request.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException exception)
            {
                return exception.Response as HttpWebResponse;
            }
        }

        private static bool IsResponseValid(HttpWebResponse response)
        {
            return response == null || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK;
        }

        private void LogErrorFrom(WebResponse response)
        {
            Stream stream = response.GetResponseStream();
            if (stream == null || stream == Stream.Null) return;

            using (var streamReader = new StreamReader(stream, true))
            {
                Log.Error(streamReader.ReadToEnd());
            }
        }
    }
}