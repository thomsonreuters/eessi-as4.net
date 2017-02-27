using System.IO;
using System.Net;
using Eu.EDelivery.AS4.Model.Notify;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    internal class HttpNotifySender : NotifySender
    {
        protected override void SendNotifyMessage(NotifyMessageEnvelope notifyMessage, string destinationUri)
        {
            // TODO: verify if destinationUri is a valid http endpoint.                        
            var request = CreateWebRequest(destinationUri, notifyMessage.ContentType);

            Log.Info($"Send Notification {notifyMessage.MessageInfo.MessageId} to {destinationUri}");

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(notifyMessage.NotifyMessage, 0, notifyMessage.NotifyMessage.Length);
            }

            // Get the response and log what we have received.
            HandleNotifyResponse(request);
        }

        private async void HandleNotifyResponse(HttpWebRequest request)
        {
            HttpWebResponse response = null;

            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;

                if (response == null)
                {
                    Log.Error("No WebResponse received for http notification.");
                    return;
                }
            }
            catch (WebException exception)
            {
                response = exception.Response as HttpWebResponse;
            }

            if (response.StatusCode != HttpStatusCode.Accepted && response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error($"Unexpected response received for http notification: {response.StatusCode}");

                if (Log.IsErrorEnabled)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream(), true))
                    {
                        Log.Error(streamReader.ReadToEnd());
                    }
                }
            }
        }


        private HttpWebRequest CreateWebRequest(string targetUrl, string contentType)
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
    }
}
