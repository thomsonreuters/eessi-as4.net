using System;
using System.IO;
using System.Net;
using System.Net.Http;
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
        public async Task<HttpWebResponse> SendPdfAsync()
        {
            await WaitToMakeSureAS4ComponentIsStartedAsync();
            HttpWebRequest webRequest = CreateWebRequest(Url, "application/pdf");
            await SendWebRequestAsync(webRequest, Properties.Resources.pdf_document);

            return await TryHandleRawResponseAsync(webRequest) as HttpWebResponse;
        }

        private static async Task<WebResponse> TryHandleRawResponseAsync(WebRequest webRequest)
        {
            try
            {
                using (WebResponse responseStream = await webRequest.GetResponseAsync())
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
        [Obsolete("Use 'SendMessageAsync' instead")]
        public async Task<AS4Message> SendMessage(string message, string contentType)
        {
            await WaitToMakeSureAS4ComponentIsStartedAsync();
            HttpWebRequest webRequest = CreateWebRequest(Url, contentType);
            await SendWebRequestAsync(webRequest, message);

            return await TryHandleWebResponse(webRequest);
        }

        /// <summary>
        /// Sends a message in the form of a 'string' with a given <paramref name="contentType"/>
        /// to a specified <paramref name="url"/>; returning the response as a deserialized <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="url">The HTTP endpoint to which the message should be sent.</param>
        /// <param name="message">The message representation to be sent.</param>
        /// <param name="contentType">The type of the message.</param>
        public async Task<AS4Message> SendMessageAsync(string url, string message, string contentType)
        {
            await WaitToMakeSureAS4ComponentIsStartedAsync();
            HttpWebRequest webRequest = CreateWebRequest(url, contentType);
            await SendWebRequestAsync(webRequest, message);

            return await TryHandleWebResponse(webRequest);
        }

        /// <summary>
        /// Sends a MIME AS4 Message
        /// which is not signed, compressed or encrypted
        /// </summary>
        /// <param name="message"></param>
        /// <param name="contentType"></param>
        public async Task<AS4Message> SendMessageAsync(string message, string contentType)
        {
            await WaitToMakeSureAS4ComponentIsStartedAsync();
            HttpWebRequest webRequest = CreateWebRequest(Url, contentType);
            await SendWebRequestAsync(webRequest, message);

            return await TryHandleWebResponse(webRequest);
        }

        private async Task SendWebRequestAsync(WebRequest webRequest, byte[] content)
        {
            Console.WriteLine($@"Send Web Request to: {Url}");
            using (Stream requestStream = await webRequest.GetRequestStreamAsync())
            {                
                var memoryStream = new MemoryStream(content);
                memoryStream.WriteTo(requestStream);
            }
        }

        /// <summary>
        /// Sends the given message stream with <paramref name="contentType"/> to the AS4.NET Component.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns></returns>
        [Obsolete("Use 'SendMessageAsync' instead")]
        public WebResponse SendMessage(Stream message, string contentType)
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
            HttpWebRequest webRequest = CreateWebRequest(Url, contentType);
            SendWebRequest(webRequest, message);

            return TryHandleRawResponse(webRequest);
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

        private async Task WaitToMakeSureAS4ComponentIsStartedAsync()
        {
            await PollingService.PollUntilPresentAsync(
                async () =>
                {
                    HttpWebRequest req = WebRequest.CreateHttp(Url);
                    req.Method = HttpMethod.Get.Method;
                    req.Accept = "text/html";

                    try
                    {
                        using (var response = (HttpWebResponse) await req.GetResponseAsync())
                        {
                            return response.StatusCode;
                        }
                    }
                    catch (WebException ex)
                    {
                        using (var response = (HttpWebResponse) ex.Response)
                        {
                            return response.StatusCode;
                        }
                    }
                },
                status => status == HttpStatusCode.OK,
                TimeSpan.FromSeconds(30));
        }

        private HttpWebRequest CreateWebRequest(string url, string contentType)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = contentType;
            request.KeepAlive = false;
            request.Connection = "Open";
            request.ProtocolVersion = HttpVersion.Version11;
            ServicePointManager.Expect100Continue = false;

            return request;
        }

        private void SendWebRequest(WebRequest webRequest, Stream message)
        {
            Console.WriteLine($@"Send Web Request to: {Url}");
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                message.CopyTo(requestStream);
            }
        }

        private async Task SendWebRequestAsync(WebRequest webRequest, string message)
        {
            Console.WriteLine($@"Send Web Request to: {Url}");
            using (Stream requestStream = await webRequest.GetRequestStreamAsync())
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return AS4Message.Empty;
            }
        }

        private async Task<AS4Message> HandleWebResponse(HttpWebRequest webRequest)
        {
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                if (HandleResponse != null)
                {
                    return HandleResponse(webResponse);
                }

                return await GetAS4ResponseAsync(webResponse as HttpWebResponse);
            }
        }

        private async Task<AS4Message> HandleWebExceptionAsync(WebException webException)
        {
            if (HandleResponse != null)
            {
                return HandleResponse(webException.Response);
            }

            return await GetAS4ResponseAsync(webException.Response as HttpWebResponse);
        }

        private static async Task<AS4Message> GetAS4ResponseAsync(HttpWebResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                return AS4Message.Empty;
            }

            string contentType = response.ContentType;
            Stream responseStream = response.GetResponseStream();

            return await new SoapEnvelopeSerializer().DeserializeAsync(responseStream, contentType);
        }
    }
}