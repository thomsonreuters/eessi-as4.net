using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// 
    /// </summary>
    internal class AS4Response : IAS4Response
    {
        private readonly HttpWebResponse _httpWebResponse;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Response" /> class.
        /// </summary>
        /// <param name="requestMessage">The resulted Message.</param>
        /// <param name="webResponse">The web Response.</param>
        private AS4Response(MessagingContext requestMessage, HttpWebResponse webResponse)
        {
            _httpWebResponse = webResponse;
            OriginalRequest = requestMessage;
        }

        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        public HttpStatusCode StatusCode => _httpWebResponse?.StatusCode ?? HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets the Message from the AS4 response.
        /// </summary>
        public AS4Message ReceivedAS4Message { get; private set; }

        public ReceivedMessage ReceivedStream { get; private set; }

        /// <summary>
        /// Gets the Original Request from this response.
        /// </summary>
        public MessagingContext OriginalRequest { get; }

        /// <summary>
        /// Create a new <see cref="AS4Response"/> instance.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <param name="webResponse"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<AS4Response> Create(MessagingContext requestMessage, HttpWebResponse webResponse, CancellationToken cancellation)
        {
            var response = new AS4Response(requestMessage, webResponse);

            var responseStream = webResponse.GetResponseStream() ?? Stream.Null;

            var contentStream = VirtualStream.CreateVirtualStream(webResponse.ContentLength);

            await responseStream.CopyToAsync(contentStream);
            contentStream.Position = 0;

            response.ReceivedStream = new ReceivedMessage(contentStream, webResponse.ContentType);
            response.ReceivedAS4Message = await TryDeserializeReceivedStream(response.ReceivedStream, cancellation);

            return response;
        }

        private static async Task<AS4Message> TryDeserializeReceivedStream(ReceivedMessage receivedStream, CancellationToken cancellation)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(receivedStream.ContentType))
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("No ContentType set - returning an empty AS4 response.");

                        var streamReader = new StreamReader(receivedStream.RequestStream);
                        string responseContent = await streamReader.ReadToEndAsync();

                        Logger.Info(responseContent);
                    }

                    return AS4Message.Empty;
                }

                var serializer = SerializerProvider.Default.Get(receivedStream.ContentType);
                return await serializer.DeserializeAsync(receivedStream.RequestStream, receivedStream.ContentType, cancellation);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                return AS4Message.Empty;
            }
            finally
            {
                receivedStream.RequestStream.Position = 0;
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _httpWebResponse?.Dispose();
        }
    }

    /// <summary>
    /// Contract to define the HTTP/AS4 response being handled.
    /// </summary>
    public interface IAS4Response : IDisposable
    {
        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the an AS4Message representation of the response.
        /// </summary>
        AS4Message ReceivedAS4Message { get; }

        /// <summary>
        /// Gets a Stream that contains the response like it has been received.
        /// </summary>
        ReceivedMessage ReceivedStream { get; }

        /// <summary>
        /// Gets the Original Request from this response.
        /// </summary>
        MessagingContext OriginalRequest { get; }
    }
}