using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using NLog;

using Function =
    System.Func
    <Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.InternalMessage>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Receiver which listens on a given target URL
    /// </summary>
    public class HttpReceiver : IReceiver
    {
        private readonly ILogger _logger;
        private readonly ISerializerProvider _provider;
        private HttpListener _listener;
        private IDictionary<string, string> _properties;

        private string Prefix => this._properties.ReadMandatoryProperty("Url");

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpReceiver"/> class. 
        /// to receive HTTP requests
        /// </summary>
        public HttpReceiver()
        {
            this._provider = new Registry().SerializerProvider;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Configure the receiver with a given Property Dictionary
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            this._properties = properties;
        }

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public async void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            // TODO: for performance : call GetContextAsync multiple times to handle concurrent requests.

            _listener = new HttpListener();

            try
            {
                _listener.Prefixes.Add(this.Prefix);
                StartListener(_listener);

                while (_listener.IsListening && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        HttpListenerContext context = await _listener.GetContextAsync();

                        await ProcessRequestAsync(context, messageCallback, cancellationToken);
                    }
                    catch (HttpListenerException)
                    {
                        this._logger.Trace($"Http Listener on {Prefix} stopped receiving requests.");
                    }
                    catch (ObjectDisposedException)
                    {
                        this._logger.Trace($"Http Listener on {Prefix} stopped receiving requests.");
                    }
                }
            }
            finally
            {
                _listener.Close();
            }
        }

        public void StopReceiving()
        {
            this._logger.Debug($"Stop listening on {Prefix}");

            if (_listener != null)
            {
                this._listener.Close();
            }
        }

        private void StartListener(HttpListener listener)
        {
            try
            {
                this._logger.Debug($"Start receiving on '{this.Prefix}'...");
                listener.Start();
            }
            catch (HttpListenerException exception)
            {
                this._logger.Error($"Http Listener Exception: {exception.Message}");
            }
        }

        private async Task ProcessRequestAsync(
            HttpListenerContext context, Function messageCallback, CancellationToken token)
        {
            this._logger.Info($"Received {context.Request.HttpMethod} request at {context.Request.RawUrl}");

            if (context.Request.HttpMethod.Equals("GET"))
            {
                DisplayHtmlStatusPage(context);
                return;
            }

            ReceivedMessage receivedMessage = CreateReceivedMessage(context);
            InternalMessage internalMessage = await messageCallback(receivedMessage, CancellationToken.None);
            ProcessResponse(context, internalMessage, token);
        }

        private static void DisplayHtmlStatusPage(HttpListenerContext context)
        {
            var responseBuilder = HttpGetResponseFactory.GetHttpGetResponse(context.Request.AcceptTypes);

            byte[] response;

            try
            {
                response = responseBuilder.GetResponse(context);
                context.Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                response = System.Text.Encoding.UTF8.GetBytes(ex.Message);
            }

            var responseLength = response.Length;

            context.Response.ContentLength64 = responseLength;
            context.Response.OutputStream.Write(response, 0, responseLength);
            context.Response.OutputStream.Close();
        }

        private static ReceivedMessage CreateReceivedMessage(HttpListenerContext context)
        {
            var requestStream = new MemoryStream();
            context.Request.InputStream.CopyTo(requestStream);
            requestStream.Position = 0;

            return new ReceivedMessage(context.Request.RawUrl, requestStream, context.Request.ContentType);
        }

        private void ProcessResponse(
           HttpListenerContext context, InternalMessage internalMessage, CancellationToken token)
        {
            SetupResponseHeaders(context, internalMessage);
            SetupResponseContent(context, internalMessage, token);
            context.Response.Close();
        }

        private static void SetupResponseHeaders(HttpListenerContext context, InternalMessage internalMessage)
        {
            context.Response.KeepAlive = false;
            context.Response.StatusCode = GetHttpStatusCode(internalMessage);

            if (internalMessage.AS4Message == null || internalMessage.AS4Message.IsEmpty)
            {
                return;
            }

            context.Response.ContentType = internalMessage.AS4Message.ContentType;
        }

        private static int GetHttpStatusCode(InternalMessage internalMessage)
        {
            var statusCode = (int)HttpStatusCode.OK;

            if (internalMessage.AS4Message?.ReceivingPMode != null && IsAS4MessageAnError(internalMessage))
            {
                statusCode = internalMessage.AS4Message.ReceivingPMode.ErrorHandling.ResponseHttpCode;
            }

            return statusCode < 100 ? 500 : statusCode;
        }

        private void SetupResponseContent(
            HttpListenerContext context, InternalMessage internalMessage, CancellationToken token)
        {
            if (internalMessage.AS4Message == null || internalMessage.AS4Message.IsEmpty)
            {
                context.Response.StatusCode = 202;
                this._logger.Info("Empty Http Body is send");
                return;
            }

            TrySerializeResponseContent(context, internalMessage, token);
        }

        private void TrySerializeResponseContent(
            HttpListenerContext context, InternalMessage internalMessage, CancellationToken token)
        {
            try
            {
                Stream responseStream = context.Response.OutputStream;
                ISerializer serializer = this._provider.Get(internalMessage.AS4Message.ContentType);
                serializer.Serialize(internalMessage.AS4Message, responseStream, token);
                responseStream.Close();
            }
            catch (System.Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private static bool IsAS4MessageAnError(InternalMessage internalMessage)
        {
            return internalMessage.AS4Message.PrimarySignalMessage is Error;
        }

        private static class HttpGetResponseFactory
        {
            public static HttpGetResponse GetHttpGetResponse(string[] acceptHeaders)
            {
                if (acceptHeaders.Contains("text/html", StringComparer.OrdinalIgnoreCase))
                {
                    return new HttpHtmlGetResponse();
                }
                if (acceptHeaders.Any(h => h.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return new HttpImageGetResponse();
                }
                throw new NotSupportedException("No HttpGetResponse implementation available for these acceptheaders.");
            }

            private sealed class HttpHtmlGetResponse : HttpGetResponse
            {
                public override byte[] GetResponse(HttpListenerContext context)
                {
                    var logoLocation = context.Request.RawUrl.TrimEnd('/') + "/assets/as4logo.png";
                         
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
                    return System.Text.Encoding.UTF8.GetBytes(html);
                }
            }

            private sealed class HttpImageGetResponse : HttpGetResponse
            {
                public override byte[] GetResponse(HttpListenerContext context)
                {
                    string file = context.Request.Url.ToString().Replace(context.Request.UrlReferrer.ToString(), "./");

                    if (File.Exists(file) == false)
                    {
                        return new byte[] { };
                    }

                    return File.ReadAllBytes(file);
                }
            }

            public abstract class HttpGetResponse
            {
                public abstract byte[] GetResponse(HttpListenerContext context);
            }
        }


    }
}