using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Eu.EDelivery.AS4.Receivers.Http.Get
{
    internal class GetHtmlHandler : IHttpGetHandler
    {
        /// <summary>
        /// Determines if the incoming request can be handled by this instance.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanHandle(HttpListenerRequest request)
        {
            string[] acceptHeaders = request.AcceptTypes;
            return acceptHeaders == null || acceptHeaders.Contains("text/html", StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handle the incoming request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public HttpResult Handle(HttpListenerRequest request)
        {
            string logoLocation = request.RawUrl.TrimEnd('/') + "/assets/as4logo.png";
            string html =
                $@"<html>
                    <head>
                        <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
                        <title>AS4.NET</title>       
                    </head>
                    <body>
                        <img src=""{logoLocation}"" alt=""AS4.NET logo"" Style=""width:100%; height:auto; display:block, margin:auto""></img>
                        <div Style=""text-align:center""><p>This AS4.NET MessageHandler is online</p></div>
                    </body>";

            return HttpResult.FromBytes(HttpStatusCode.OK, Encoding.UTF8.GetBytes(html), "text/html");
        }
    }
}