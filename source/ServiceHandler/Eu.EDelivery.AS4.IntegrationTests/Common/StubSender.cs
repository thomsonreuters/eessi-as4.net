using System;
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
        private readonly ISerializer _serializer;
        public string Url { get; set; } = $"http://localhost:8081/msh/";
        public Func<WebResponse, AS4Message> HandleResponse { get; set; }

        public StubSender()
        {
            var soapSerializer = new SoapEnvelopeSerializer();
            this._serializer = new MimeMessageSerializer(soapSerializer);
        }

        public StubSender(ISerializer serializer)
        {
            this._serializer = serializer;
        }

        /// <summary>
        /// Sends a MIME AS4 Message
        /// which is not signed, compressed or encrypted
        /// </summary>
        public Task<AS4Message> SendAsync(string message, string contentType)
        {
            WaitToMakeSureAS4ComponentIsStarted();
            HttpWebRequest webRequest = CreateWebRequest(contentType);
            SendWebRequest(webRequest, message);

            return TryHandleWebResponse(webRequest);
        }

        private void WaitToMakeSureAS4ComponentIsStarted()
        {
            Thread.Sleep(3000);
        }

        private HttpWebRequest CreateWebRequest(string contentType)
        {
            var request = WebRequest.Create(this.Url) as HttpWebRequest;
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
            Console.WriteLine($@"Send Web Request to: {this.Url}");
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
                if (this.HandleResponse != null)
                    return this.HandleResponse(responseStream);

                return await GetAS4ResponseAsync(responseStream);
            }
        }

        private async Task<AS4Message> GetAS4ResponseAsync(WebResponse response)
        {
            string contentType = response.ContentType;
            Stream responseStream = response.GetResponseStream();

            return await new SoapEnvelopeSerializer()
                .DeserializeAsync(responseStream, contentType, CancellationToken.None);
        }

        private async Task<AS4Message> HandleWebExceptionAsync(WebException webException)
        {
            if (this.HandleResponse != null)
                return this.HandleResponse(webException.Response);

            return await GetAS4ResponseAsync(webException.Response);
        }
    }
}