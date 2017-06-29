using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.ComponentTests.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Deserializes the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        public static async Task<AS4Message> DeserializeToAS4Message(this HttpResponseMessage response)
        {
            ISerializer serializer = SerializerProvider.Default.Get(response.Content.Headers.ContentType.MediaType);

            return await serializer.DeserializeAsync(
                       await response.Content.ReadAsStreamAsync(),
                       response.Content.Headers.ContentType.MediaType,
                       CancellationToken.None);
        }
    }
}
