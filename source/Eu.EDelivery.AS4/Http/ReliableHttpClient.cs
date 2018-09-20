using System;
using System.Net;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Http
{
    internal class ReliableHttpClient : IHttpClient
    {
        /// <summary>
        /// Request a Message for the <see cref="IHttpClient"/> implementation.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public HttpWebRequest Request(string url, string contentType)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            request.KeepAlive = false;
            request.Connection = "Open";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ServicePoint.ConnectionLimit = 12 * Environment.ProcessorCount;

            ServicePointManager.Expect100Continue = false;

            return request;
        }

        /// <summary>
        /// Send a <see cref="HttpWebRequest"/> to the configured target.
        /// </summary>
        /// <param name="request">To be send <see cref="HttpWebRequest"/>.</param>
        /// <returns></returns>
        public async Task<(HttpWebResponse response, WebException exception)> Respond(HttpWebRequest request)
        {
            try
            {
                WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);

                return (response as HttpWebResponse, exception: null);
            }
            catch (WebException exception)
            {
                return (exception.Response as HttpWebResponse, exception);
            }
        }
    }

    public interface IHttpClient
    {
        /// <summary>
        /// Send a <see cref="HttpWebRequest"/> to the configured target.
        /// </summary>
        /// <param name="request">To be send <see cref="HttpWebRequest"/>.</param>
        /// <returns></returns>
        Task<(HttpWebResponse response, WebException exception)> Respond(HttpWebRequest request);

        /// <summary>
        /// Request a Message for the <see cref="IHttpClient"/> implementation.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        HttpWebRequest Request(string url, string contentType);
    }
}
