using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Http;
using Eu.EDelivery.AS4.Receivers.Http.Get;
using Eu.EDelivery.AS4.Receivers.Http.Post;
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
    [Info("HTTP receiver")]
    public sealed class HttpReceiver : IReceiver, IDisposable
    {
        private HttpListener _listener;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        [Info("Url", required: true)]
        [Description("The URL to receive messages on. The url can also contain a port ex: http://localhost:5000/msh/")]
        private string Url { get; set; }

        [Info("Maximum concurrent requests to process", defaultValue: 10)]
        [Description("Indicates how wany requests should be processed per batch.")]
        private int ConcurrentRequests { get; set; }

        [Info("Use logging", defaultValue: false)]
        [Description("Log incoming requests to logs\\receivedmessages\\.")]
        private bool UseLogging { get; set; }

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
        /// Configure the receiver with a given settings dictionary.
        /// </summary>
        /// <param name="settings"></param>
        public void Configure(IEnumerable<Setting> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            Dictionary<string, string> properties = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

            const int defaultConcurrentRequests = 10;
            string concurrentRequestValue = properties.ReadOptionalProperty(SettingKeys.ConcurrentRequests, defaultConcurrentRequests.ToString());
            if (int.TryParse(concurrentRequestValue, out int maxConcurrentConnections))
            {
                ConcurrentRequests = maxConcurrentConnections;
            }
            else
            {
                Logger.Warn($"Invalid \"{SettingKeys.ConcurrentRequests}\" was given: {concurrentRequestValue}, will fall back to \"{defaultConcurrentRequests}\"");
                ConcurrentRequests = defaultConcurrentRequests;
            }

            string useLoggingValue = properties.ReadOptionalProperty(SettingKeys.UseLogging, defaultValue: false.ToString());
            bool.TryParse(useLoggingValue, out bool useLogging);
            UseLogging = useLogging;

            string hostname = properties.ReadMandatoryProperty(SettingKeys.Url);
            if (hostname.EndsWith("/") == false)
            {
                Url = hostname + "/";
            }
            else
            {
                Url = hostname;
            }
        }

        /// <summary>
        /// Start receiving on a configured Target
        /// Received messages will be send to the given Callback
        /// </summary>
        /// <param name="messageCallback"></param>
        /// <param name="cancellationToken"></param>
        public void StartReceiving(Function messageCallback, CancellationToken cancellationToken)
        {
            if (messageCallback == null)
            {
                throw new ArgumentNullException(nameof(messageCallback));
            }

            _listener = new HttpListener();

            try
            {
                _listener.Prefixes.Add(Url);
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

                Logger.Debug($"Start receiving on \"{Url}\" ...");
                Logger.Debug($"      with max concurrent connections = {ConcurrentRequests}");
                Logger.Debug($"      with logging = {UseLogging}");
            }
            catch (HttpListenerException exception)
            {
                Logger.Error($"Http Listener Exception: {exception.Message}");
            }
        }

        private void AcceptConnections(
            HttpListener listener,
            Function messageCallback,
            CancellationToken cancellation)
        {
            Router router = new Router()
                .Via(new GetHtmlHandler())
                .Via(new GetImageHandler())
                .Via(new ExceptionPostHandler())
                .Via(new SubmitPostHandler())
                .Via(new AsyncSignalResponseHandler())
                .Via(new ForwardMessageResponseHandler())
                .Via(new PullRequestResponseHandler())
                .Via(new SyncSignalResponseHandler());

            GuardMaxConcurrentHttpConnections(
                listener,
                cancellation,
                processRequestAsync: async context =>
                {
                    Logger.Info($"Received {context.Request.HttpMethod} request at \"{context.Request.RawUrl}\"");
                    await router.RouteWithAsync(
                        httpContext: context,
                        prePostSelector: req => RunRequestThroughAgentAsync(req, messageCallback));

                    context.Response.Close();
                });
        }

        private void GuardMaxConcurrentHttpConnections(
            HttpListener listener, 
            CancellationToken cancellationToken,
            Func<HttpListenerContext, Task> processRequestAsync)
        {
            // The Semaphore makes sure the the maximum amount of concurrent connections is respected.
            using (var semaphore = new Semaphore(ConcurrentRequests, ConcurrentRequests))
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
                        listener.GetContextAsync()
                                .ContinueWith(async httpContextTask =>
                                {
                                    // A request is being handled, so decrease the semaphore which will allow 
                                    // that we're listening on another context.
                                    semaphore.Release();

                                    HttpListenerContext context = await httpContextTask.ConfigureAwait(false);

                                    await processRequestAsync(context).ConfigureAwait(false);

                                    context.Response.Close();
                                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed                 
                    }
                    catch (HttpListenerException)
                    {
                        Logger.Trace($"Http Listener on {Url} stopped receiving requests.");
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

        private async Task<MessagingContext> RunRequestThroughAgentAsync(HttpListenerRequest request, Function messageCallback)
        {

            ReceivedMessage message = await CreateReceivedMessageAsync(request).ConfigureAwait(false);
            try
            {
                return await messageCallback(message, CancellationToken.None)
                            .ConfigureAwait(false);
            }
            finally
            {
                message.UnderlyingStream.Dispose();
            }
        }

        private async Task<ReceivedMessage> CreateReceivedMessageAsync(HttpListenerRequest request)
        {
            ReceivedMessage message = await WrapRequestInSeekableMessageAsync(request, request.ContentLength64);

            if (UseLogging)
            {
                await LogReceivedMessageMessageAsync(message, request.Url).ConfigureAwait(false);
            }

            return message;
        }

        private static async Task<ReceivedMessage> WrapRequestInSeekableMessageAsync(HttpListenerRequest request, long contentLength)
        {
            Logger.Trace("Start copying to VirtualStream");
            var dest = new VirtualStream(
                request.ContentLength64 > VirtualStream.ThresholdMax
                    ? VirtualStream.MemoryFlag.OnlyToDisk
                    : VirtualStream.MemoryFlag.AutoOverFlowToDisk,
                forAsync: true);

            if (contentLength > 0)
            {
                dest.SetLength(contentLength);
            }

            await request.InputStream
                         .CopyToFastAsync(dest)
                         .ConfigureAwait(false);

            dest.Position = 0;

            return new ReceivedMessage(
                underlyingStream: dest,
                contentType: request.ContentType,
                origin: request.UserHostAddress,
                length: request.ContentLength64);
        }

        private static async Task LogReceivedMessageMessageAsync(ReceivedMessage message, Uri url)
        {
            const string logDir = @".\logs\receivedmessages\";

            if (Directory.Exists(logDir) == false)
            {
                Directory.CreateDirectory(logDir);
            }

            string hostInformation;

            try
            {
                hostInformation = $"{url.Host}_{url.Port}";
            }
            catch
            {
                hostInformation = "localhost";
            }

            try
            {
                string newReceivedMessageFile =
                    FilenameUtils.EnsureValidFilename($"{hostInformation}.{Guid.NewGuid()}.{DateTime.Now:yyyyMMdd}");

                Logger.Info($"Logging to \"{newReceivedMessageFile}\"");

                using (FileStream destinationStream =
                    FileUtils.CreateAsync(
                        Path.Combine(logDir, newReceivedMessageFile),
                        FileOptions.SequentialScan))
                {
                    await message.UnderlyingStream
                                 .CopyToFastAsync(destinationStream)
                                 .ConfigureAwait(false);
                }

                message.UnderlyingStream.Position = 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Stop the <see cref="IReceiver"/> instance from receiving.
        /// </summary>
        public void StopReceiving()
        {
            Logger.Debug($"Stop listening on \"{Url}\"");

            _listener?.Close();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage(
            category: "Microsoft.Usage", 
            checkId: "CA2213:DisposableFieldsShouldBeDisposed", 
            MessageId = "_listener",
            Justification = "Warning but not justified")]
        public void Dispose()
        {
            try
            {
                ((IDisposable) _listener)?.Dispose();
            }
            catch (Exception exception)
            {
                Logger.Debug(exception.Message);
            }
        }
    }
}