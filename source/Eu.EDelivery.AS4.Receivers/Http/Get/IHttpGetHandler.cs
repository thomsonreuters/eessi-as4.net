using System.Net;

namespace Eu.EDelivery.AS4.Receivers.Http.Get
{
    internal interface IHttpGetHandler
    {
        /// <summary>
        /// Determines if the incoming request can be handled by this instance.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        bool CanHandle(HttpListenerRequest request);

        /// <summary>
        /// Handle the incoming request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        HttpResult Handle(HttpListenerRequest request);
    }
}