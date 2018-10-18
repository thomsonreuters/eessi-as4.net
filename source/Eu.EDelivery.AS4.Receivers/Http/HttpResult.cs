using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Receivers.Http.Get;
using Eu.EDelivery.AS4.Receivers.Http.Post;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Receivers.Http
{
    /// <summary>
    /// Result of the <see cref="Router"/> of the request through the <see cref="IHttpGetHandler"/>s and <see cref="IHttpPostHandler"/>s.
    /// </summary>
    internal class HttpResult
    {
        private readonly HttpStatusCode _status;
        private readonly string _contentType;
        private readonly Func<HttpListenerResponse, Task> _writeToAsync;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        internal HttpResult(
            HttpStatusCode status,
            string contentType,
            Func<HttpListenerResponse, Task> writeToAsync)
        {
            _status = status;
            _contentType = contentType;
            _writeToAsync = writeToAsync;
        }

        /// <summary>
        /// Creates a new empty result with only a status code.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static HttpResult Empty(HttpStatusCode status)
        {
            return FromBytes(status, new byte[0], String.Empty);
        }

        /// <summary>
        /// Creates a new empty result with status and content type.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpResult Empty(HttpStatusCode status, string contentType)
        {
            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            return FromBytes(status, new byte[0], contentType);
        }

        /// <summary>
        /// Creates a new result from a series of bytes.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpResult FromBytes(HttpStatusCode status, byte[] content, string contentType)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            return new HttpResult(
                status,
                contentType,
                async response =>
                {
                    response.ContentLength64 = content.Length;
                    await response.OutputStream.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
                });
        }

        /// <summary>
        /// Creates a new result from a stream.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpResult FromStream(HttpStatusCode status, Stream content, string contentType)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            return new HttpResult(
                status,
                contentType,
                async response =>
                {
                    StreamUtilities.MovePositionToStreamStart(content);
                    await content.CopyToFastAsync(response.OutputStream);
                });
        }

        /// <summary>
        /// Creates a new result based on an <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static HttpResult FromAS4Message(HttpStatusCode status, AS4Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new HttpResult(
                status,
                message.ContentType,
                async response => await WriteAS4MessageToResponseAsync(message, response));
        }

        private static async Task WriteAS4MessageToResponseAsync(AS4Message message, HttpListenerResponse response)
        {
            try
            {
                using (Stream responseStream = response.OutputStream)
                {
                    if (message.IsEmpty == false)
                    {
                        ISerializer serializer = SerializerProvider.Default.Get(message.ContentType);

                        await serializer.SerializeAsync(message, responseStream);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error($"An error occured while writing the Response to the ResponseStream: {exception.Message}");
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace(exception.StackTrace);
                }

                throw;
            }
        }

        /// <summary>
        /// Write the configured contents to the specified HTTP response.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task WriteToAsync(HttpListenerResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            response.StatusCode = (int)_status;
            response.ContentType = _contentType;
            response.KeepAlive = false;

            await _writeToAsync(response);
        }
    }
}