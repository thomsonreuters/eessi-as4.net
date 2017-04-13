using System.Net;

namespace Eu.EDelivery.AS4.Http
{
    /// <summary>
    /// <see cref="HttpWebRequest"/> creator for a persistent instance throughout the AS4 Component.
    /// </summary>
    public class HttpPostRequest
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="HttpPostRequest"/> class from being created.
        /// </summary>
        private HttpPostRequest() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequest"/> class 
        /// for a specified <paramref name="url"/> scheme and <paramref name="contentType"/>.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpWebRequest Create(string url, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
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
