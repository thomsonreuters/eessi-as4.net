using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.TestUtils.Stubs
{
    public class StubSender
    {
        private static readonly HttpClient _client = new HttpClient();

        public static async Task<HttpResponseMessage> SendAS4Message(string url, AS4Message as4Message)
        {
            var request = await CreatePostRequestMessage(url, as4Message);
            return await _client.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> SendRequest(string url, byte[] content, string contentType)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new ByteArrayContent(content)
            };

            message.Content.Headers.Add("Content-Type", contentType);

            return await _client.SendAsync(message);
        }

        private static async Task<HttpRequestMessage> CreatePostRequestMessage(string sendToUrl, AS4Message message)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, sendToUrl);

            byte[] serializedMessage;

            using (var stream = new MemoryStream())
            {
                ISerializer serializer = SerializerProvider.Default.Get(message.ContentType);
                await serializer.SerializeAsync(message, stream, CancellationToken.None);

                serializedMessage = stream.ToArray();
            }

            requestMessage.Content = new ByteArrayContent(serializedMessage);
            requestMessage.Content.Headers.Add("Content-Type", message.ContentType);

            return requestMessage;
        }
    }
}
