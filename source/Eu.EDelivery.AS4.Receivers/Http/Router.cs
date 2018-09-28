using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers.Http.Get;
using Eu.EDelivery.AS4.Receivers.Http.Post;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;
using NLog;
using ExecStepsFunc =
    System.Func<Eu.EDelivery.AS4.Model.Internal.ReceivedMessage, System.Threading.CancellationToken,
        System.Threading.Tasks.Task<Eu.EDelivery.AS4.Model.Internal.MessagingContext>>;

namespace Eu.EDelivery.AS4.Receivers.Http
{
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
            _postHandlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Start the routing of the incoming request through the registered handlers.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="func"></param>
        /// <param name="useLogging"></param>
        /// <returns></returns>
        public async Task RouteAsync(
            HttpListenerContext context, 
            ExecStepsFunc func, 
            bool useLogging)
        {
            MessagingContext stepResult = null;

            try
            {
                if (func != null && StringComparer.OrdinalIgnoreCase.Equals(context.Request.HttpMethod, "POST"))
                {
                    ReceivedMessage receivedMessage = await CreateReceivedMessageAsync(context.Request, useLogging)
                        .ConfigureAwait(false);
                    try
                    {
                        stepResult =
                            await func(receivedMessage, CancellationToken.None)
                                .ConfigureAwait(false);
                    }
                    finally
                    {
                        receivedMessage.UnderlyingStream.Dispose();
                    }
                }

                Maybe<HttpResult> handlerResultM = HandleRequestWithConfiguredHandlers(context.Request, stepResult);
                await handlerResultM.DoAsync(r => r.WriteToAsync(context.Response));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                stepResult?.Dispose();
            }
        }

        private Maybe<HttpResult> HandleRequestWithConfiguredHandlers(HttpListenerRequest request, MessagingContext stepResult)
        {
            if (request.HttpMethod == HttpMethod.Get.Method)
            {
                return _getHandlers
                    .FirstOrNothing(h => h.CanHandle(request))
                    .Select(h => h.Handle(request))
                    .OrElse(() =>
                    {
                        Logger.Debug("Respond with 202 Accepted: unknown reason");
                        return HttpResult.Empty(HttpStatusCode.NotAcceptable, "text/plain");
                    });
            }

            if (request.HttpMethod == HttpMethod.Post.Method)
            {
                return _postHandlers
                    .FirstOrNothing(h => h.CanHandle(stepResult))
                    .Select(h => h.Handle(stepResult))
                    .OrElse(() => HttpResult.Empty(HttpStatusCode.Accepted));
            }

            return HttpResult
                .Empty(HttpStatusCode.MethodNotAllowed)
                .AsMaybe();
        }

        private static async Task<ReceivedMessage> CreateReceivedMessageAsync(HttpListenerRequest request, bool useLogging)
        {
            ReceivedMessage message = await WrapRequestInSeekableMessageAsync(request, request.ContentLength64);

            if (useLogging)
            {
                await LogReceivedMessageMessageAsync(message, request.Url).ConfigureAwait(false);
            }

            return message;
        }

        private static async Task<ReceivedMessage> WrapRequestInSeekableMessageAsync(
            HttpListenerRequest request,
            long contentLength)
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
    }
}
