using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.IntegrationTests.Common
{
    /// <summary>
    /// Simulation (Stub) af a MSH
    /// </summary>
    public class StubSender
    {
        public string Url { get; set; } = $"http://localhost:8081/msh/";

        public Func<WebResponse, AS4Message> HandleResponse { get; set; }

        /// <summary>
        /// Send a resource to the AS4 Component.
        /// </summary>
        /// <returns></returns>
        public HttpWebResponse SendPdf()
        {
            WaitToMakeSureAS4ComponentIsStarted();
            HttpWebRequest webRequest = CreateWebRequest("application/pdf");
            SendWebRequest(webRequest, Properties.Resources.pdf_document);

            return TryHandleRawResponse(webRequest) as HttpWebResponse;
        }

        private static WebResponse TryHandleRawResponse(WebRequest webRequest)
        {
            try
            {
                using (WebResponse responseStream = webRequest.GetResponse())
                {
                    return responseStream;
                }
            }
            catch (WebException exception)
            {
                Console.WriteLine(exception.Message);
                return exception.Response;
            }
        }

        /// <summary>
        /// Sends a MIME AS4 Message
        /// which is not signed, compressed or encrypted
        /// </summary>
        /// <param name="message"></param>
        /// <param name="contentType"></param>
        public Task<AS4Message> SendMessage(string message, string contentType)
        {
            WaitToMakeSureAS4ComponentIsStarted();
            HttpWebRequest webRequest = CreateWebRequest(contentType);
            SendWebRequest(webRequest, message);

            return TryHandleWebResponse(webRequest);
        }

        private static void WaitToMakeSureAS4ComponentIsStarted()
        {
            Thread.Sleep(3000);
        }

        private HttpWebRequest CreateWebRequest(string contentType)
        {
            var request = WebRequest.Create(Url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = contentType;
            request.KeepAlive = false;
            request.Connection = "Open";
            request.ProtocolVersion = HttpVersion.Version11;
            ServicePointManager.Expect100Continue = false;

            return request;
        }

        private void SendWebRequest(WebRequest webRequest, string message)
        {
            Console.WriteLine($@"Send Web Request to: {Url}");
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                var memoryStream = new MemoryStream(messageBytes);
                memoryStream.WriteTo(requestStream);
            }
        }

        private async Task<AS4Message> TryHandleWebResponse(HttpWebRequest webRequest)
        {
            try
            {
                return await HandleWebResponse(webRequest);
            }
            catch (WebException webException)
            {
                return await HandleWebExceptionAsync(webException);
            }
        }

        private async Task<AS4Message> HandleWebResponse(HttpWebRequest webRequest)
        {
            using (WebResponse responseStream = webRequest.GetResponse())
            {
                if (HandleResponse != null)
                {
                    return HandleResponse(responseStream);
                }

                return await GetAS4ResponseAsync(responseStream);
            }
        }

        private async Task<AS4Message> HandleWebExceptionAsync(WebException webException)
        {
            if (HandleResponse != null)
            {
                return HandleResponse(webException.Response);
            }

            return await GetAS4ResponseAsync(webException.Response);
        }

        private static async Task<AS4Message> GetAS4ResponseAsync(WebResponse response)
        {
            string contentType = response.ContentType;
            Stream responseStream = response.GetResponseStream();

            return await new SoapEnvelopeSerializer().DeserializeAsync(responseStream, contentType, CancellationToken.None);
        }
    }
}