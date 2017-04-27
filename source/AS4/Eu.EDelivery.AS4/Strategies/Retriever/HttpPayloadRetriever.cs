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
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _sendRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPayloadRetriever" /> class.
        /// </summary>
        public HttpPayloadRetriever() : this(HttpClient.SendAsync) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPayloadRetriever" /> class.
        /// </summary>
        /// <param name="sendRequest"></param>
        public HttpPayloadRetriever(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendRequest)
        {
            _sendRequest = sendRequest;
        }

        /// <summary>
        /// Retrieve the payload from the given location
        /// </summary>
        /// <param name="location"> The location. </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"><paramref name="location" /> is null. </exception>
        /// <exception cref="UriFormatException">
        /// In the .NET for Windows Store apps or the Portable Class Library, catch the base
        /// class exception, <see cref="T:System.FormatException" />, instead.<paramref name="location" /> is empty.-or- The scheme
        /// specified in <paramref name="location" /> is not correctly formed. See
        /// <see cref="M:System.Uri.CheckSchemeName(System.String)" />.-or- <paramref name="location" /> contains too many
        /// slashes.-or- The password specified in <paramref name="location" /> is not valid.-or- The host name specified in
        /// <paramref name="location" /> is not valid.-or- The file name specified in <paramref name="location" /> is not valid.
        /// -or- The user name specified in <paramref name="location" /> is not valid.-or- The host or authority name specified in
        /// <paramref name="location" /> cannot be terminated by backslashes.-or- The port number specified in
        /// <paramref name="location" /> is not valid or cannot be parsed.-or- The length of <paramref name="location" /> exceeds
        /// 65519 characters.-or- The length of the scheme specified in <paramref name="location" /> exceeds 1023 characters.-or-
        /// There is an invalid character sequence in <paramref name="location" />.-or- The MS-DOS path specified in
        /// <paramref name="location" /> must start with c:\\.
        /// </exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<Stream> RetrievePayloadAsync(string location)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(location));

            HttpResponseMessage response = await _sendRequest(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return Stream.Null;
            }

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
    }
}