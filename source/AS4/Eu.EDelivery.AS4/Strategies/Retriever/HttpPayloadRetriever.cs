using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Strategies.Retriever
{
    /// <summary>
    /// Web Retriever Implementation to retrieve the RequestStream of a external file
    /// </summary>
    public class HttpPayloadRetriever : IPayloadRetriever
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// Retrieve the payload from the given location
        /// </summary>
        /// <param name="location"> The location. </param>
        /// <returns> </returns>
        public async Task<Stream> RetrievePayloadAsync(string location)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(location));

            HttpResponseMessage response = await HttpClient.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return Stream.Null;
            }

            return await response.Content.ReadAsStreamAsync();
        }
    }
}