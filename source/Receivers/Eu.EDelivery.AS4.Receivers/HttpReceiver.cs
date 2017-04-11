using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.InternalMessage>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Receiver which listens on a given target URL
    /// </summary>
    public class HttpReceiver : IReceiver
    {

        private readonly ILogger _logger;
        private HttpListener _listener;
        private IDictionary<string, string> _properties;

        private string Prefix => _properties.ReadMandatoryProperty(SettingKeys.Url);

        private int _maxConcurrentConnections;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpReceiver"/> class. 
        /// to receive HTTP requests
        /// </summary>
        public HttpReceiver()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }
        private static class SettingKeys
        {
            public const string Url = "Url";
            public const string ConcurrentRequests = "ConcurrentRequests";
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            _properties = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

            _maxConcurrentConnections = Convert.ToInt32(_properties.ReadOptionalProperty(SettingKeys.ConcurrentRequests, "10"));
        }

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            _listener = new HttpListener();

            try
            {
                _listener.Prefixes.Add(Prefix);
                StartListener(_listener);

                AcceptConnections(_listener, messageCallback, cancellationToken);
            }
            finally
            {
                _listener.Close();
            }
        }

        private void AcceptConnections(HttpListener listener, Function messageCallback, CancellationToken cancellationToken)
        {
            // The Semaphore makes sure the the maximum amount of concurrent connections is respected.
            using (var semaphore = new Semaphore(_maxConcurrentConnections, _maxConcurrentConnections))
            {
                while (listener.IsListening && !cancellationToken.IsCancellationRequested)
                {
                    semaphore.WaitOne();

                    try
                    {
                        if (listener.IsListening == false)
                        {
                            return;
                        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        listener.GetContextAsync().ContinueWith(async (c) =>
                        {
                            // A request is being handled, so decrease the semaphore which will allow 
                            // that we're listening on another context.
                            semaphore.Release();

                            HttpListenerContext context = await c;

                            await ProcessRequestAsync(context, messageCallback);
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed                 
                    }
                    catch (HttpListenerException)
                    {
                        _logger.Trace($"Http Listener on {Prefix} stopped receiving requests.");
                    }
                    catch (ObjectDisposedException)
                    {
                        // Not doing anything on purpose.
                        // When a HttpListener is stopped, the context where being listened on is called one more time, 
                        // but the context is disposed.  Therefore an exception is thrown.  Catch the exception to prevent
                        // the process to end, but do nothing with the exception since this is by design.
                    }
                }
            }
        }

        public void StopReceiving()
        {
            _logger.Debug($"Stop listening on {Prefix}");

            _listener?.Close();
        }

        private void StartListener(HttpListener listener)
        {
            try
            {
                _logger.Debug($"Start receiving on '{Prefix}'...");
                listener.Start();
            }
            catch (HttpListenerException exception)
            {
                _logger.Error($"Http Listener Exception: {exception.Message}");
            }
        }

        private async Task ProcessRequestAsync(
            HttpListenerContext context,
            Function messageCallback)
        {
            _logger.Info($"Received {context.Request.HttpMethod} request at {context.Request.RawUrl}");

            var handler = RequestHandler.GetHandler(context.Request);

            var handleResult = await handler.ExecuteAsync(context.Request, messageCallback);

            handleResult.ExecuteResult(context.Response);

            context.Response.Close();
        }

        #region Inner RequestHandler classes

        private abstract class RequestHandler
        {
            public static RequestHandler GetHandler(HttpListenerRequest request)
            {
                if (request.HttpMethod == "GET")
                {
                    return GetRequestHandler.Create(request);
                }

                if (request.HttpMethod == "POST")
                {
                    return PostRequestHandler.Default;
                }

                return new ErrorRequestHandler(HttpStatusCode.MethodNotAllowed, "");
            }

            public async Task<HttpListenerContentResult> ExecuteAsync(HttpListenerRequest request, Function processor)
            {
                InternalMessage processorResult = null;

                if (processor != null && request.HttpMethod == "POST")
                {
                    var receivedMessage = CreateReceivedMessage(request);
                    processorResult = await processor(receivedMessage, CancellationToken.None);
                }

                return ExecuteCore(request, processorResult);
            }

            protected abstract HttpListenerContentResult ExecuteCore(HttpListenerRequest request, InternalMessage processorResult);

            private static ReceivedMessage CreateReceivedMessage(HttpListenerRequest request)
            {
                return new ReceivedMessage(request.RawUrl, request.InputStream, request.ContentType);                
            }

            #region Concrete RequestHandler implementations

            private abstract class GetRequestHandler : RequestHandler
            {
                public static RequestHandler Create(HttpListenerRequest request)
                {
                    var acceptHeaders = request.AcceptTypes;

                    if (acceptHeaders == null || acceptHeaders.Contains("text/html", StringComparer.OrdinalIgnoreCase))
                    {
                        return HttpHtmlGetHandler.Default;
                    }

                    if (acceptHeaders.Any(h => h.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return HttpImageGetHandler.Default;
                    }

                    return new ErrorRequestHandler(HttpStatusCode.NotAcceptable, string.Empty);
                }

                private sealed class HttpHtmlGetHandler : GetRequestHandler
                {
                    public static readonly HttpHtmlGetHandler Default = new HttpHtmlGetHandler();

                    protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, InternalMessage processorResult)
                    {
                        string logoLocation = request.RawUrl.TrimEnd('/') + "/assets/as4logo.png";

                        string html = $@"<html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    <title>AS4.NET</title>       
</head>
<body>
    <img src=""{logoLocation}"" alt=""AS4.NET logo"" Style=""width:100%; height:auto; display:block, margin:auto""></img>
    <div Style=""text-align:center""><p>This AS4.NET MessageHandler is online</p></div>
</body>";

                        return new ByteContentResult(HttpStatusCode.OK, "text/html", Encoding.UTF8.GetBytes(html));
                    }
                }

                private sealed class HttpImageGetHandler : GetRequestHandler
                {
                    public static readonly HttpImageGetHandler Default = new HttpImageGetHandler();

                    protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, InternalMessage processorResult)
                    {
                        string file = request.Url.ToString().Replace(request.UrlReferrer.ToString(), "./");

                        if (File.Exists(file) == false)
                        {
                            return new ByteContentResult(HttpStatusCode.NotFound, string.Empty, new byte[] { });
                        }

                        return new ByteContentResult(HttpStatusCode.OK, "image/jpeg", File.ReadAllBytes(file));
                    }
                }
            }

            private class ErrorRequestHandler : RequestHandler
            {
                private readonly HttpStatusCode _status;
                private readonly string _message;

                /// <summary>
                /// Initializes a new instance of the <see cref="ErrorRequestHandler"/> class.
                /// </summary>
                public ErrorRequestHandler(HttpStatusCode statusCode, string message)
                {
                    _status = statusCode;
                    _message = message;
                }

                protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, InternalMessage processorResult)
                {
                    return new ByteContentResult(_status, "text/plain", Encoding.UTF8.GetBytes(_message));
                }
            }

            private class PostRequestHandler : RequestHandler
            {

                public static readonly PostRequestHandler Default = new PostRequestHandler();

                protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, InternalMessage processorResult)
                {
                    if (processorResult.Exception != null)
                    {
                        return new ByteContentResult(HttpStatusCode.InternalServerError, "text/plain", Encoding.UTF8.GetBytes(processorResult.Exception.Message));
                    }

                    // Ugly hack until the Transformer is refactored.
                    // When the InternalMessage contains a non-empty SubmitMessage, we assume that a message has been submitted and we should respond accordingly.
                    if (processorResult.SubmitMessage?.IsEmpty == false)
                    {
                        if (processorResult.AS4Message?.IsEmpty == false)
                        {
                            return new ByteContentResult(HttpStatusCode.Accepted, string.Empty, new byte[] { });
                        }
                    }

                    // This statement will evaluate to true when we're sending receipts or errors in callback (async) mode.
                    if ((processorResult.AS4Message == null || processorResult.AS4Message.IsEmpty) && processorResult.Exception == null)
                    {
                        return new ByteContentResult(HttpStatusCode.Accepted, string.Empty, new byte[] { });
                    }
                    
                    // This statement will evaluate to true when receipts or errors are sent in respond mode.
                    if (processorResult.AS4Message != null && processorResult.AS4Message.IsEmpty == false)
                    {
                        HttpStatusCode statusCode = HttpStatusCode.OK;

                        if (processorResult.AS4Message?.ReceivingPMode != null && IsAS4MessageAnError(processorResult))
                        {
                            if (Enum.IsDefined(typeof(HttpStatusCode), processorResult.AS4Message.ReceivingPMode.ErrorHandling.ResponseHttpCode))
                            {
                                statusCode = (HttpStatusCode)processorResult.AS4Message.ReceivingPMode.ErrorHandling.ResponseHttpCode;
                            }
                            else
                            {
                                statusCode = HttpStatusCode.InternalServerError;
                            }
                        }

                        return new AS4MessageContentResult(statusCode, processorResult.AS4Message.ContentType, processorResult);
                    }

                    // In any other case, return a bad request ?
                    return new ByteContentResult(HttpStatusCode.BadRequest, "", new byte[] { });

                }

                private static bool IsAS4MessageAnError(InternalMessage internalMessage)
                {
                    return internalMessage.AS4Message.PrimarySignalMessage is Error;
                }
            }

            #endregion
        }

        #endregion

        #region Inner ContentResult classes

        private abstract class HttpListenerContentResult
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _contentType;

            /// <summary>
            /// Initializes a new instance of the <see cref="HttpListenerContentResult"/> class.
            /// </summary>
            protected HttpListenerContentResult(HttpStatusCode statusCode, string contentType)
            {
                _statusCode = statusCode;
                _contentType = contentType;
            }

            public void ExecuteResult(HttpListenerResponse response)
            {
                response.StatusCode = (int)_statusCode;
                response.ContentType = _contentType;
                response.KeepAlive = false;
                ExecuteResultCore(response);
            }

            protected abstract void ExecuteResultCore(HttpListenerResponse response);
        }

        private class ByteContentResult : HttpListenerContentResult
        {
            private readonly byte[] _content;

            /// <summary>
            /// Initializes a new instance of the <see cref="ByteContentResult"/> class.
            /// </summary>
            public ByteContentResult(HttpStatusCode statusCode, string contentType, byte[] content) : base(statusCode, contentType)
            {
                _content = content;
            }

            protected override void ExecuteResultCore(HttpListenerResponse response)
            {
                response.ContentLength64 = _content.Length;
                response.OutputStream.Write(_content, 0, _content.Length);
            }
        }

        private class AS4MessageContentResult : HttpListenerContentResult
        {
            private readonly InternalMessage _internalMessage;

            private static readonly ISerializerProvider SerializerProvider = new Registry().SerializerProvider;

            /// <summary>
            /// Initializes a new instance of the <see cref="AS4MessageContentResult"/> class.
            /// </summary>
            public AS4MessageContentResult(HttpStatusCode statusCode, string contentType, InternalMessage internalMessage) : base(statusCode, contentType)
            {
                _internalMessage = internalMessage;
            }

            protected override void ExecuteResultCore(HttpListenerResponse response)
            {
                using (Stream responseStream = response.OutputStream)
                {
                    if (_internalMessage.AS4Message?.IsEmpty == false)
                    {
                        var serializer = SerializerProvider.Get(_internalMessage.AS4Message.ContentType);
                        serializer.Serialize(_internalMessage.AS4Message, responseStream, CancellationToken.None);
                    }
                }
            }
        }

        #endregion
    }
}