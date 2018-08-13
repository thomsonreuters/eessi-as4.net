using System.Web.Http;
using Eu.EDelivery.AS4.Singletons;

namespace AS4.ParserService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            AS4Mapper.Initialize();
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
