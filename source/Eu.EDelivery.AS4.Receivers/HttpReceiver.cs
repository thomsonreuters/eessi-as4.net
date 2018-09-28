using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Http;
using Eu.EDelivery.AS4.Receivers.Http.Get;
using Eu.EDelivery.AS4.Receivers.Http.Post;
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
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private HttpRequestMeta _requestMeta;
        private HttpListener _listener;
        private int _maxConcurrentConnections;
        private Dictionary<string, string> _properties;

        [Info("Url", required: true)]
        [Description("The URL to receive messages on. The url can also contain a port ex: http://localhost:5000/msh/")]
        private string Url => _properties?.ReadOptionalProperty(SettingKeys.Url);

        [Info("Maximum concurrent requests to process", defaultValue: 10)]
        [Description("Indicates how wany requests should be processed per batch.")]
        private int ConcurrentRequests => Convert.ToInt32(_properties?.ReadOptionalProperty(SettingKeys.ConcurrentRequests, "10"));

        [Info("Use logging")]
        [Description("Log incoming requests to logs\\receivedmessages\\.")]
        private bool UseLogging => Convert.ToBoolean(_properties?.ReadOptionalProperty(SettingKeys.UseLogging, "false"));

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
                if (hostname.EndsWith("/") == false)
                {
                    Hostname = hostname + "/";
                }
                else
                {
                    Hostname = hostname;
                }
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
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _properties = settings.ToDictionary(s => s.Key, s => s.Value, StringComparer.OrdinalIgnoreCase);

            const int defaultConcurrentRequests = 10;
            string concurrentRequestValue = _properties.ReadOptionalProperty(SettingKeys.ConcurrentRequests, defaultConcurrentRequests.ToString());
            if (!int.TryParse(concurrentRequestValue, out _maxConcurrentConnections))
            {
                Logger.Warn($"Invalid \"{SettingKeys.ConcurrentRequests}\" was given: {concurrentRequestValue}, will fall back to \"{defaultConcurrentRequests}\"");
                _maxConcurrentConnections = defaultConcurrentRequests;
            }

            string useLoggingValue = _properties.ReadOptionalProperty(SettingKeys.UseLogging, defaultValue: false.ToString());
            bool.TryParse(useLoggingValue, out var useLogging);

            string hostname = _properties.ReadMandatoryProperty(SettingKeys.Url);
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
            if (messageCallback == null)
            {
                throw new ArgumentNullException(nameof(messageCallback));
            }

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

                Logger.Debug($"Start receiving on \"{_requestMeta.Hostname}\" ...");
                Logger.Debug($"      with max concurrent connections = {_maxConcurrentConnections}");
                Logger.Debug($"      with logging = {_requestMeta.UseLogging}");
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
            Logger.Info($"Received {context.Request.HttpMethod} request at \"{context.Request.RawUrl}\"");

            await new Router()
                .Via(new GetHtmlHandler())
                .Via(new GetImageHandler())
                .Via(new ExceptionPostHandler())
                .Via(new SubmitPostHandler())
                .Via(new AsyncSignalResponseHandler())
                .Via(new ForwardMessageResponseHandler())
                .Via(new PullRequestResponseHandler())
                .Via(new SyncSignalResponseHandler())
                .RouteAsync(context, messageCallback, _requestMeta.UseLogging);

            context.Response.Close();
        }

        /// <summary>
        /// Stop the <see cref="IReceiver"/> instance from receiving.
        /// </summary>
        public void StopReceiving()
        {
            Logger.Debug($"Stop listening on \"{_requestMeta.Hostname}\"");

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