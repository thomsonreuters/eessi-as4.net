using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Http.Get;
using Eu.EDelivery.AS4.Receivers.Http.Post;
using NLog;

namespace Eu.EDelivery.AS4.Receivers.Http
{
    /// <summary>
    /// HTTP request routing hub to handle the request in a both maintainable and extensible way.
    /// </summary>
    internal class Router
    {
        private readonly Collection<IHttpGetHandler> _getHandlers;
        private readonly Collection<IHttpPostHandler> _postHandlers;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="Router"/> class.
        /// </summary>
        public Router()
        {
            _getHandlers = new Collection<IHttpGetHandler>();
            _postHandlers = new Collection<IHttpPostHandler>();
        }

        /// <summary>
        /// Adds a handler for GET requests to the router.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Router Via(IHttpGetHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _getHandlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Adds a handler for POST requests to the router.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Router Via(IHttpPostHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _postHandlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Start the routing of the incoming request through the registered handlers.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="prePostSelector"></param>
        /// <returns></returns>
        public async Task RouteWithAsync(
            HttpListenerContext httpContext,
            Func<HttpListenerRequest, Task<MessagingContext>> prePostSelector)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (httpContext.Request == null)
            {
                throw new ArgumentNullException(nameof(httpContext.Request), @"Routing requres a HTTP request");
            }

            HttpListenerRequest request = httpContext.Request;
            HttpListenerResponse response = httpContext.Response;

            try
            {
                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    await _getHandlers
                        .FirstOrNothing(h => h.CanHandle(request))
                        .Select(h => h.Handle(request))
                        .OrElse(() =>
                        {
                            Logger.Debug("Respond with 202 Accepted: unknown reason");
                            return HttpResult.Empty(HttpStatusCode.NotAcceptable, "text/plain");
                        })
                        .DoAsync(r => r.WriteToAsync(response));
                }

                if (request.HttpMethod == HttpMethod.Post.Method)
                {
                    if (prePostSelector == null)
                    {
                        throw new ArgumentNullException(
                            nameof(prePostSelector),
                            @"Requires a selector function when the request is a HTTP POST request");
                    }

                    using (MessagingContext agentResult = await prePostSelector(request))
                    {
                        // TODO: is this a correct way to dispose the MessagingContext with an asynchronous HTTP response write?
                        await _postHandlers
                            .FirstOrNothing(h => h.CanHandle(agentResult))
                            .Select(h => h.Handle(agentResult))
                            .OrElse(() => HttpResult.Empty(HttpStatusCode.Accepted))
                            .DoAsync(r => r.WriteToAsync(response));
                    }
                }

                await HttpResult
                    .Empty(HttpStatusCode.MethodNotAllowed)
                    .AsMaybe()
                    .DoAsync(r => r.WriteToAsync(response));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
