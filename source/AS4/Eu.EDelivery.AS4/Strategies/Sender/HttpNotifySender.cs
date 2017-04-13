using System.IO;
using System.Net;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Sender
{
    internal class HttpNotifySender : INotifySender
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private Method _method;

        /// <summary>
        /// Configure the <see cref="INotifySender"/>
        /// with a given <paramref name="method"/>
        /// </summary>
        /// <param name="method"></param>
        public void Configure(Method method)
        {
            _method = method;
        }

        /// <summary>
        /// Send a given <paramref name="message"/> to a given endpoint
        /// </summary>
        /// <param name="message">The message.</param>
        public async void Send(NotifyMessageEnvelope message)
        {
            string destinationUri = _method["location"].Value;
            HttpWebRequest request = CreateNotifyRequest(message, destinationUri);
            HttpWebResponse response = await SendNotifyRequest(request);

            response?.Close();
        }

        private static HttpWebRequest CreateNotifyRequest(NotifyMessageEnvelope notifyMessage, string destinationUri)
        {
            // TODO: verify if destinationUri is a valid http endpoint.
            HttpWebRequest request = HttpPostRequest.Create(destinationUri, notifyMessage.ContentType);

            Logger.Info($"Send Notification {notifyMessage.MessageInfo.MessageId} to {destinationUri}");

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(notifyMessage.NotifyMessage, 0, notifyMessage.NotifyMessage.Length);
            }

            return request;
        }

        private static async Task<HttpWebResponse> SendNotifyRequest(WebRequest request)
        {
            HttpWebResponse response;

            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;

                if (response == null)
                {
                    Logger.Error("No WebResponse received for http notification.");
                }
            }
            catch (WebException exception)
            {
                response = exception.Response as HttpWebResponse;
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