using System.Collections.Generic;
using System.IO;
using System.Net;
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
            var listener = new HttpListener();
            listener.Prefixes.Add(this.Prefix);
            StartListener(listener);

            while (listener.IsListening && !cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                await ProcessRequestAsync(context, messageCallback, cancellationToken);
            }

            listener.Close();
        }

        private void StartListener(HttpListener listener)
        {
            try
            {
                this._logger.Info($"Start receiving on '{this.Prefix}'...");
                listener.Start();
            }
            catch (HttpListenerException exception)
            {
                this._logger.Error("Http Listener Exception");
                this._logger.Debug($"Http Listener Exception: {exception.Message}");
            }
        }

        private async Task ProcessRequestAsync(
            HttpListenerContext context, Function messageCallback, CancellationToken token)
        {
            if (context.Request.HttpMethod.Equals("GET")) return;

            this._logger.Info($"Received request at: {this.Prefix}");

            ReceivedMessage receivedMessage = CreateReceivedMessage(context);
            InternalMessage internalMessage = await messageCallback(receivedMessage, CancellationToken.None);
            ProcessResponse(context, internalMessage, token);
        }

        private ReceivedMessage CreateReceivedMessage(HttpListenerContext context)
        {
            var requestStream = new MemoryStream();
            context.Request.InputStream.CopyTo(requestStream);
            requestStream.Position = 0;

            return new ReceivedMessage(requestStream, context.Request.ContentType);
        }

        private void ProcessResponse(
           HttpListenerContext context, InternalMessage internalMessage, CancellationToken token)
        {
            SetupResponseHeaders(context, internalMessage);
            SetupResponseContent(context, internalMessage, token);
            context.Response.Close();
        }

        private void SetupResponseHeaders(HttpListenerContext context, InternalMessage internalMessage)
        {
            context.Response.KeepAlive = false;
            context.Response.StatusCode = GetHttpStatusCode(internalMessage);

            if (internalMessage.AS4Message.IsEmpty) return;
            context.Response.ContentType = internalMessage.AS4Message.ContentType;
        }

        private int GetHttpStatusCode(InternalMessage internalMessage)
        {
            var statusCode = (int)HttpStatusCode.OK;
            if (internalMessage.AS4Message.ReceivingPMode != null && IsAS4MessageAnError(internalMessage))
                statusCode = internalMessage.AS4Message.ReceivingPMode.ErrorHandling.ResponseHttpCode;

            const int defaultStatusCode = 500;
            bool isStatusCodeValid = statusCode < 100;
            return isStatusCodeValid ? defaultStatusCode : statusCode;
        }

        private void SetupResponseContent(
            HttpListenerContext context, InternalMessage internalMessage, CancellationToken token)
        {
            if (internalMessage.AS4Message.IsEmpty)
            {
                context.Response.StatusCode = 202;
                this._logger.Info("Empty Http Body is send");
                return;
            }

            LogInformation(internalMessage);
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
            }
            catch (System.Exception exception)
            {
                this._logger.Error(exception.Message);
            }
        }

        private void LogInformation(InternalMessage internalMessage)
        {
            string type = IsAS4MessageAnError(internalMessage) ? nameof(Error) : nameof(Receipt);
            this._logger.Info($"AS4 {type} is send to requested party");
        }

        private bool IsAS4MessageAnError(InternalMessage internalMessage)
        {
            return internalMessage.AS4Message.PrimarySignalMessage is Error;
        }
    }
}