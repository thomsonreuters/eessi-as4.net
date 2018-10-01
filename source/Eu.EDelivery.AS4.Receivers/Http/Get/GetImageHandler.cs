using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Eu.EDelivery.AS4.Receivers.Http.Get
{
    /// <summary>
    /// HTTP GET handler to respond with the image of the component.
    /// </summary>
    internal class GetImageHandler : IHttpGetHandler
    {
        /// <summary>
        /// Determines if the incoming request can be handled by this instance.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanHandle(HttpListenerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.AcceptTypes?.Any(h => h.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Handle the incoming request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public HttpResult Handle(HttpListenerRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string file = request.Url.ToString().Replace(request.UrlReferrer.ToString(), "./");

            if (File.Exists(file) == false)
            {
                return HttpResult.Empty(HttpStatusCode.NotFound);
            }

            return HttpResult.FromBytes(HttpStatusCode.OK, File.ReadAllBytes(file), "image/jpeg");
        }
    }
}