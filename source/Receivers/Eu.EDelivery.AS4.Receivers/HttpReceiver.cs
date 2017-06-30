using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;
using NLog;
using Function =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.MessagingContext>>;

namespace Eu.EDelivery.AS4.Receivers
{
    /// <summary>
    /// Receiver which listens on a given target URL
    /// </summary>
    public sealed class HttpReceiver : IReceiver, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private HttpRequestMeta _requestMeta;
        private HttpListener _listener;
        private int _maxConcurrentConnections;

        /// <summary>
        /// Data Class that contains the required keys to correctly configure the <see cref="HttpReceiver"/>.
        /// </summary>
        private static class SettingKeys
        {
            public const string Url = "Url";
            public const string ConcurrentRequests = "MaxConcurrentRequests";
            public const string UseLogging = "UseLogging";
        }

        /// <summary>
        /// Meta info about the <see cref="HttpListenerRequest"/>.
        /// </summary>
        private class HttpRequestMeta
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HttpRequestMeta" /> class.
            /// </summary>
            /// <param name="hostname">The hostname.</param>
            /// <param name="useLogging">if set to <c>true</c> [use logging].</param>
            public HttpRequestMeta(string hostname, bool useLogging)
            {
                Hostname = hostname;
                UseLogging = useLogging;
            }

            /// <summary>
            /// Gets  the hostname.
            /// </summary>
            /// <value>The hostname.</value>
            public string Hostname { get; }

            /// <summary>
            /// Gets a value indicating whether [use logging].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [use logging]; otherwise, <c>false</c>.
            /// </value>
            public bool UseLogging { get; }
        }

        /// <summary>
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            Dictionary<string, string> properties = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

            const int defaultConcurrentRequests = 10;
            string concurrentRequestValue = properties.ReadOptionalProperty(SettingKeys.ConcurrentRequests, defaultConcurrentRequests.ToString());
            if (!int.TryParse(concurrentRequestValue, out _maxConcurrentConnections))
            {
                Logger.Warn($"Invalid '{SettingKeys.ConcurrentRequests}' was given: {concurrentRequestValue}, will fall back to '{defaultConcurrentRequests}'");
                _maxConcurrentConnections = defaultConcurrentRequests;
            }

            string useLoggingValue = properties.ReadOptionalProperty(SettingKeys.UseLogging, defaultValue: false.ToString());
            bool.TryParse(useLoggingValue, out var useLogging);

            string hostname = properties.ReadMandatoryProperty(SettingKeys.Url);
            _requestMeta = new HttpRequestMeta(hostname, useLogging);
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
                _listener.Prefixes.Add(_requestMeta.Hostname);
                StartListener(_listener);

                AcceptConnections(_listener, messageCallback, cancellationToken);
            }
            finally
            {
                _listener.Close();
            }
        }

        private void StartListener(HttpListener listener)
        {
            try
            {                
                listener.Start();

                Logger.Debug($"Start receiving on '{_requestMeta.Hostname}'...");
            }
            catch (HttpListenerException exception)
            {
                Logger.Error($"Http Listener Exception: {exception.Message}");
            }
        }

        private void AcceptConnections(
            HttpListener listener,
            Function messageCallback,
            CancellationToken cancellationToken)
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
                        listener.GetContextAsync().ContinueWith(
                            async httpContextTask =>
                            {
                                // A request is being handled, so decrease the semaphore which will allow 
                                // that we're listening on another context.
                                semaphore.Release();

                                HttpListenerContext context = await httpContextTask.ConfigureAwait(false);

                                await ProcessRequestAsync(context, messageCallback).ConfigureAwait(false);
                            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed                 
                    }
                    catch (HttpListenerException)
                    {
                        Logger.Trace($"Http Listener on {_requestMeta.Hostname} stopped receiving requests.");
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

        private async Task ProcessRequestAsync(HttpListenerContext context, Function messageCallback)
        {
            Logger.Info($"Received {context.Request.HttpMethod} request at {context.Request.RawUrl}");

            RequestHandler handler = RequestHandler.GetHandler(context.Request);
            HttpListenerContentResult handleResult =
                await handler.ExecuteAsync((_requestMeta, context.Request), messageCallback).ConfigureAwait(false);

            await handleResult.ExecuteResultAsync(context.Response).ConfigureAwait(false);

            context.Response.Close();
        }

        #region Inner RequestHandler classes

        /// <summary>
        /// Abstraction for handling HTTP requests.
        /// </summary>
        private abstract class RequestHandler
        {
            /// <summary>
            /// Factory Method to get the right <see cref="RequestHandler"/> based on the given <paramref name="request"/>.
            /// </summary>
            /// <param name="request"></param>
            /// <returns></returns>
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

                return new ErrorRequestHandler(HttpStatusCode.MethodNotAllowed, string.Empty);
            }

            /// <summary>
            /// Handle the <see cref="HttpListenerRequest"/>.
            /// </summary>
            /// <param name="request"></param>
            /// <param name="processor"></param>
            /// <returns></returns>
            /// <exception cref="Exception">A delegate callback throws an exception.</exception>
            public async Task<HttpListenerContentResult> ExecuteAsync((HttpRequestMeta settings, HttpListenerRequest value) request, Function processor)
            {
                MessagingContext processorResult = null;

                try
                {
                    if (processor != null && request.value.HttpMethod == "POST")
                    {
                        ReceivedMessage receivedMessage = await CreateReceivedMessage(request).ConfigureAwait(false);
                        try
                        {
                            processorResult =
                                await processor(receivedMessage, CancellationToken.None).ConfigureAwait(false);
                        }
                        finally
                        {
                            receivedMessage.RequestStream.Dispose();
                        }
                    }

                    return ExecuteCore(request.value, processorResult);
                }
                finally
                {
                   processorResult?.Dispose();
                }
            }

            private static async Task<ReceivedMessage> CreateReceivedMessage((HttpRequestMeta settings, HttpListenerRequest value) request)
            {
                HttpListenerRequest requestValue = request.value;

                if (request.settings.UseLogging)
                {
                    VirtualStream.MemoryFlag flag = requestValue.ContentLength64 > VirtualStream.ThresholdMax
                        ? VirtualStream.MemoryFlag.OnlyToDisk
                        : VirtualStream.MemoryFlag.AutoOverFlowToDisk;

                    ReceivedMessage message = await RequestAsVirtualStreamMessage(requestValue, flag);
                    await LogReceivedMessageMessage(message, request.settings.Hostname);

                    return message;
                }

                if (requestValue.ContentLength64 > VirtualStream.ThresholdMax)
                {
                    return await RequestAsVirtualStreamMessage(requestValue, VirtualStream.MemoryFlag.OnlyToDisk);
                }

                return new ReceivedMessage(requestValue.InputStream, requestValue.ContentType);
            }

            private static async Task<ReceivedMessage> RequestAsVirtualStreamMessage(
                HttpListenerRequest request,
                VirtualStream.MemoryFlag flag)
            {
                var destinationStream = new VirtualStream(flag);
                await request.InputStream.CopyToAsync(destinationStream).ConfigureAwait(false);
                destinationStream.Position = 0;

                return new ReceivedMessage(destinationStream, request.ContentType);
            }

            private static async Task LogReceivedMessageMessage(ReceivedMessage message, string hostname)
            {
                const string logDir = @".\logs\receivedmessages\";

                if (Directory.Exists(logDir) == false)
                {
                    Directory.CreateDirectory(logDir);
                }

                UriBuilder uriBuilder = new UriBuilder(hostname);

                string newReceivedMessageFile = FilenameSanitizer.EnsureValidFilename(Path.Combine(logDir, $"{uriBuilder.Path}_{uriBuilder.Port}.{Guid.NewGuid()}.{DateTime.Now:yyyyMMdd}"));

                using (var destinationStream = new FileStream(Path.Combine(logDir, newReceivedMessageFile), FileMode.Create))
                {
                    await message.RequestStream.CopyToAsync(destinationStream).ConfigureAwait(false);
                }

                message.RequestStream.Position = 0;
            }

            /// <summary>
            /// Specific <see cref="HttpListenerRequest"/> handling.
            /// </summary>
            /// <param name="request"></param>
            /// <param name="processorResult"></param>
            /// <returns></returns>
            protected abstract HttpListenerContentResult ExecuteCore(
                HttpListenerRequest request,
                MessagingContext processorResult);

            #region Concrete RequestHandler implementations

            private abstract class GetRequestHandler : RequestHandler
            {
                /// <summary>
                /// Create a <see cref="RequestHandler"/> implementation for the HTTP GET.
                /// </summary>
                /// <param name="request"></param>
                /// <returns></returns>
                public static RequestHandler Create(HttpListenerRequest request)
                {
                    string[] acceptHeaders = request.AcceptTypes;

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

                    /// <summary>
                    /// Specific <see cref="HttpListenerRequest"/> handling.
                    /// </summary>
                    /// <param name="request"></param>
                    /// <param name="processorResult"></param>
                    /// <returns></returns>
                    protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, MessagingContext processorResult)
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

                    /// <summary>
                    /// Specific <see cref="HttpListenerRequest"/> handling.
                    /// </summary>
                    /// <param name="request"></param>
                    /// <param name="processorResult"></param>
                    /// <returns></returns>
                    protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, MessagingContext processorResult)
                    {
                        string file = request.Url.ToString().Replace(request.UrlReferrer.ToString(), "./");

                        if (File.Exists(file) == false)
                        {
                            return ByteContentResult.Empty(HttpStatusCode.NotFound);
                        }

                        return new ByteContentResult(HttpStatusCode.OK, "image/jpeg", File.ReadAllBytes(file));
                    }
                }
            }

            private class ErrorRequestHandler : RequestHandler
            {
                private readonly string _message;
                private readonly HttpStatusCode _status;

                /// <summary>
                /// Initializes a new instance of the <see cref="RequestHandler.ErrorRequestHandler" /> class.
                /// </summary>
                /// <param name="statusCode">The status code.</param>
                /// <param name="message">The message.</param>
                public ErrorRequestHandler(HttpStatusCode statusCode, string message)
                {
                    _status = statusCode;
                    _message = message;
                }

                /// <summary>
                /// Specific <see cref="HttpListenerRequest"/> handling.
                /// </summary>
                /// <param name="request"></param>
                /// <param name="processorResult"></param>
                /// <returns></returns>
                protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, MessagingContext processorResult)
                {
                    return new ByteContentResult(_status, "text/plain", Encoding.UTF8.GetBytes(_message));
                }
            }

            private class PostRequestHandler : RequestHandler
            {
                public static readonly PostRequestHandler Default = new PostRequestHandler();

                private static bool IsAS4MessageAnError(MessagingContext messagingContext)
                {
                    return messagingContext.AS4Message.PrimarySignalMessage is Error;
                }

                protected override HttpListenerContentResult ExecuteCore(HttpListenerRequest request, MessagingContext processorResult)
                {
                    if (processorResult.Exception != null)
                    {
                        return
                            new ByteContentResult(
                                processorResult.ErrorResult?.Code == ErrorCode.NotApplicable
                                || processorResult.ErrorResult == null
                                    ? HttpStatusCode.BadRequest
                                    : HttpStatusCode.InternalServerError,
                                "text/plain",
                                Encoding.UTF8.GetBytes(processorResult.Exception.Message));
                    }

                    // Ugly hack until the Transformer is refactored.
                    // When we're in SubmitMode and have an Empty AS4Message, then we should return an Accepted.
                    if (processorResult.Mode == MessagingContextMode.Submit && processorResult.AS4Message?.IsEmpty == false)
                    {
                        return ByteContentResult.Empty(HttpStatusCode.Accepted);
                    }

                    if (AreReceiptsOrErrorsSendInCallbackMode(processorResult))
                    {
                        return ByteContentResult.Empty(HttpStatusCode.Accepted);
                    }

                    if (AreReceiptsOrErrorsSendInResponseMode(processorResult))
                    {
                        return new AS4MessageContentResult(
                            statusCode: DetermineHttpCodeFrom(processorResult),
                            contentType: processorResult.AS4Message?.ContentType,
                            messagingContext: processorResult);
                    }

                    // In any other case, return a bad request error ?
                    return ByteContentResult.Empty(HttpStatusCode.BadRequest);
                }

                private static bool AreReceiptsOrErrorsSendInCallbackMode(MessagingContext processorResult)
                {
                    return (processorResult.AS4Message == null || processorResult.AS4Message.IsEmpty)
                           && processorResult.Exception == null;
                }

                private static bool AreReceiptsOrErrorsSendInResponseMode(MessagingContext processorResult)
                {
                    return processorResult.AS4Message != null && processorResult.AS4Message.IsEmpty == false;
                }

                private static HttpStatusCode DetermineHttpCodeFrom(MessagingContext processorResult)
                {
                    if (processorResult?.ReceivingPMode != null && IsAS4MessageAnError(processorResult))
                    {
                        int errorHttpCode = processorResult.ReceivingPMode.ErrorHandling.ResponseHttpCode;

                        if (Enum.IsDefined(typeof(HttpStatusCode), errorHttpCode))
                        {
                            return (HttpStatusCode)errorHttpCode;
                        }

                        return HttpStatusCode.InternalServerError;
                    }

                    return HttpStatusCode.OK;
                }
            }

            #endregion
        }

        #endregion

        #region Inner ContentResult classes

        private abstract class HttpListenerContentResult
        {
            private readonly string _contentType;
            private readonly HttpStatusCode _statusCode;

            /// <summary>
            /// Initializes a new instance of the <see cref="HttpListenerContentResult" /> class.
            /// </summary>
            /// <param name="statusCode">The status code.</param>
            /// <param name="contentType">Type of the content.</param>
            protected HttpListenerContentResult(HttpStatusCode statusCode, string contentType)
            {
                _statusCode = statusCode;
                _contentType = contentType;
            }

            /// <summary>
            /// Handeling the <see cref="HttpListenerResponse"/>.
            /// </summary>
            /// <param name="response"></param>
            public async Task ExecuteResultAsync(HttpListenerResponse response)
            {
                response.StatusCode = (int)_statusCode;
                response.ContentType = _contentType;
                response.KeepAlive = false;

                await ExecuteResultAsyncCore(response).ConfigureAwait(false);
            }

            /// <summary>
            /// Specific <see cref="HttpListenerResponse"/> handling.
            /// </summary>
            /// <param name="response"></param>
            protected abstract Task ExecuteResultAsyncCore(HttpListenerResponse response);
        }

        private class ByteContentResult : HttpListenerContentResult
        {
            private readonly byte[] _content;

            /// <summary>
            /// Initializes a new instance of the <see cref="ByteContentResult" /> class.
            /// </summary>
            /// <param name="statusCode">The status code.</param>
            /// <param name="contentType">Type of the content.</param>
            /// <param name="content">The content.</param>
            public ByteContentResult(HttpStatusCode statusCode, string contentType, byte[] content)
                : base(statusCode, contentType)
            {
                _content = content;
            }

            /// <summary>
            /// Creates an empty <see cref="ByteContentResult"/> result.
            /// </summary>
            /// <param name="statusCode">Embedded <see cref="HttpStatusCode"/> in the empty result.</param>
            /// <returns></returns>
            public static ByteContentResult Empty(HttpStatusCode statusCode)
                => new ByteContentResult(statusCode, string.Empty, new byte[] { });

            /// <summary>
            /// Specific <see cref="HttpListenerResponse"/> handling.
            /// </summary>
            /// <param name="response"></param>
            protected override async Task ExecuteResultAsyncCore(HttpListenerResponse response)
            {
                response.ContentLength64 = _content.Length;
                await response.OutputStream.WriteAsync(_content, 0, _content.Length).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///   <see cref="HttpListenerContentResult" /> implementation to serialize the result as <see cref="AS4Message" />.
        /// </summary>
        /// <seealso cref="Eu.EDelivery.AS4.Receivers.HttpReceiver.HttpListenerContentResult" />
        private class AS4MessageContentResult : HttpListenerContentResult
        {
            private static readonly ISerializerProvider SerializerProvider = Serialization.SerializerProvider.Default;
            private readonly MessagingContext _messagingContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="AS4MessageContentResult" /> class.
            /// </summary>
            public AS4MessageContentResult(HttpStatusCode statusCode, string contentType, MessagingContext messagingContext)
                : base(statusCode, contentType)
            {
                _messagingContext = messagingContext;
            }

            /// <summary>
            /// Specific <see cref="HttpListenerResponse"/> handling.
            /// </summary>
            /// <param name="response"></param>
            protected override async Task ExecuteResultAsyncCore(HttpListenerResponse response)
            {
                try
                {
                    using (Stream responseStream = response.OutputStream)
                    {
                        if (_messagingContext.AS4Message?.IsEmpty == false)
                        {
                            ISerializer serializer = SerializerProvider.Get(_messagingContext.AS4Message.ContentType);

                            await serializer.SerializeAsync(
                                _messagingContext.AS4Message,
                                responseStream,
                                CancellationToken.None);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(
                        $"An error occured while writing the Response to the ResponseStream: {exception.Message}");
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace(exception.StackTrace);
                    }
                    throw;
                }
            }
        }

        #endregion

        /// <summary>
        /// Stop the <see cref="IReceiver"/> instance from receiving.
        /// </summary>
        public void StopReceiving()
        {
            Logger.Debug($"Stop listening on {_requestMeta.Hostname}");

            _listener?.Close();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_listener",
            Justification = "Warning but not justified")]
        public void Dispose()
        {
            try
            {
                ((IDisposable)_listener)?.Dispose();
            }
            catch (Exception exception)
            {
                Logger.Debug(exception.Message);
            }
        }
    }
}