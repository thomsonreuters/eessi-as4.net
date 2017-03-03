using System.IO;
using System.Net;
using Eu.EDelivery.AS4.Model.Deliver;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public class HttpDeliverySender : DeliverySender
    {
        protected override void SendDeliverMessage(DeliverMessageEnvelope deliverMessage, string destinationUri)
        {
            // TODO: verify if destinationUri is a valid http endpoint.                        
            var request = CreateWebRequest(destinationUri, deliverMessage.ContentType);

            this.Log.Info($"Send Notification {deliverMessage.MessageInfo.MessageId} to {destinationUri}");

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(deliverMessage.DeliverMessage, 0, deliverMessage.DeliverMessage.Length);
            }

            // Get the response and log what we have received.
            HandleDeliverResponse(request);
        }

        private static HttpWebRequest CreateWebRequest(string targetUrl, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(targetUrl);
            request.Method = "POST";
            request.ContentType = contentType;
            request.KeepAlive = false;
            request.Connection = "Open";
            request.ProtocolVersion = HttpVersion.Version11;

            ServicePointManager.Expect100Continue = false;

            return request;
        }

        private async void HandleDeliverResponse(HttpWebRequest request)
        {
            HttpWebResponse response = null;

            try
            {
                try
                {
                    response = await request.GetResponseAsync() as HttpWebResponse;

                    if (response == null)
                    {
                        Log.Error("No WebResponse received for http delivery.");
                        return;
                    }
                }
                catch (WebException exception)
                {
                    response = exception.Response as HttpWebResponse;
                }

                if ( response != null && response.StatusCode != HttpStatusCode.Accepted && response.StatusCode != HttpStatusCode.OK)
                {
                    Log.Error($"Unexpected response received for http delivery: {response.StatusCode}");

                    if (Log.IsErrorEnabled)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream(), true))
                        {
                            Log.Error(streamReader.ReadToEnd());
                        }
                    }
                }
            }
            finally
            {
                response?.Close();                 
            }
        }
    }
}