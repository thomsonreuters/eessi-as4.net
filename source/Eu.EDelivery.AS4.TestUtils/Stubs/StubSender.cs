using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.TestUtils.Stubs
{
    public class StubSender
    {
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Sends an AS4 Message to the endpoint that listens at the specified url.
        /// </summary>
        /// <param name="url">The url of the endpoint to send the message to.</param>
        /// <param name="as4Message">The AS4Message that must be sent.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendAS4Message(string url, AS4Message as4Message)
        {
            var request = await CreatePostRequestMessage(url, as4Message);

            Console.WriteLine($@"Send AS4Message as HTTP POST request to: {url}, Content-Type: {as4Message.ContentType}");
            return await Client.SendAsync(request);
        }

        /// <summary>
        /// Sends a request to the endpoint that listens at the specified url.
        /// </summary>
        /// <param name="url">The url of the endpoint to send the message to.</param>
        /// <param name="content">A byte array that contains the content of the request.</param>
        /// <param name="contentType">The contenttype.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendRequest(string url, byte[] content, string contentType)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new ByteArrayContent(content)
            };

            message.Content.Headers.Add("Content-Type", contentType);

            Console.WriteLine($@"Send HTTP POST request to: {url}, Content-Type: {contentType}");
            return await Client.SendAsync(message);
        }

        private static async Task<HttpRequestMessage> CreatePostRequestMessage(string sendToUrl, AS4Message message)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, sendToUrl);

            byte[] serializedMessage;

            using (var stream = new MemoryStream())
            {
                ISerializer serializer = SerializerProvider.Default.Get(message.ContentType);
                await serializer.SerializeAsync(message, stream);

                serializedMessage = stream.ToArray();
            }

            requestMessage.Content = new ByteArrayContent(serializedMessage);
            requestMessage.Content.Headers.Add("Content-Type", message.ContentType);

            return requestMessage;
        }
    }
}
