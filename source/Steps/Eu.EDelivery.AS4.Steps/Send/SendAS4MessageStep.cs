using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Send.Response;
using Eu.EDelivery.AS4.Streaming;
using Eu.EDelivery.AS4.Utilities;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Send <see cref="AS4Message" /> to the corresponding Receiving MSH
    /// </summary>
    [Description("This step makes sure that an AS4 Message that has been processed, is sent to its destination.")]
    [Info("Send AS4 Message.")]
    public class SendAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createDatastore;
        private readonly IHttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class
        /// </summary>
        public SendAS4MessageStep() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class.
        /// Create a Send AS4Message Step
        /// with a given Serializer Provider
        /// </summary>
        /// <param name="createDatastore"></param>
        public SendAS4MessageStep(Func<DatastoreContext> createDatastore)
            : this(createDatastore, new ReliableHttpClient()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4MessageStep" /> class.
        /// </summary>
        /// <param name="createDatastore">Delegate to create a new context.</param>
        /// <param name="client">Instance to handle the HTTP response.</param>
        public SendAS4MessageStep(Func<DatastoreContext> createDatastore, IHttpClient client)
        {
            _createDatastore = createDatastore;
            _httpClient = client;
        }

        /// <summary>
        /// Send the <see cref="AS4Message" />
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellation)
        {
            if (messagingContext.ReceivedMessage == null && messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException("SendAS4MessageStep requires a MessagingContext with a ReceivedStream or an AS4 Message");
            }

            if (messagingContext.ReceivedMessage == null && messagingContext.AS4Message.IsPullRequest == false)
            {
                throw new InvalidOperationException("The SendStep expects a PullRequest AS4 Message when the MessagingContext does not contain a ReceivedStream");
            }

            var sendConfiguration = messagingContext.SendingPMode.PushConfiguration;

            if (sendConfiguration == null)
            {
                throw new ConfigurationErrorsException($"The Sending PMode {messagingContext.SendingPMode.Id} does not contain a PushConfiguration element.");
            }

            var as4Message = await GetAS4MessageFromContextAsync(messagingContext);

            try
            {
                string contentType = messagingContext.ReceivedMessage?.ContentType ?? messagingContext.AS4Message.ContentType;

                contentType = contentType.Replace("charset=\"utf-8\"", "");

                HttpWebRequest request = CreateWebRequest(sendConfiguration, contentType);

                if (await TryWriteToHttpRequestStreamAsync(request, messagingContext).ConfigureAwait(false))
                {
                    messagingContext.ModifyContext(as4Message);

                    return await TryHandleHttpResponseAsync(request, messagingContext, cancellation).ConfigureAwait(false);
                }

                return StepResult.Failed(messagingContext);
            }
            catch (Exception exception)
            {
                Logger.Error(
                    $"{messagingContext.EbmsMessageId} An error occured while trying to send the message: {exception.Message}");
                Logger.Trace(exception.StackTrace);

                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }

                throw;
            }
            finally
            {
                await UpdateMessageStatusAsync(as4Message, Operation.Sent, OutStatus.Sent).ConfigureAwait(false);
            }
        }

        private HttpWebRequest CreateWebRequest(ISendConfiguration sendConfiguration, string contentType)
        {
            Logger.Info($"Creating WebRequest to {sendConfiguration.Protocol.Url}");

            HttpWebRequest request = _httpClient.Request(sendConfiguration.Protocol.Url, contentType);

            AssignClientCertificate(sendConfiguration.TlsConfiguration, request);

            return request;
        }

        private static void AssignClientCertificate(TlsConfiguration configuration, HttpWebRequest request)
        {
            if (!configuration.IsEnabled || configuration.ClientCertificateInformation == null)
            {
                return;
            }

            Logger.Info("Adding Client TLS Certificate to Http Request.");

            X509Certificate2 certificate = RetrieveTlsCertificate(configuration);

            request.ClientCertificates.Add(certificate);
        }

        private static X509Certificate2 RetrieveTlsCertificate(TlsConfiguration configuration)
        {
            var certFindCriteria = configuration.ClientCertificateInformation as ClientCertificateReference;

            if (certFindCriteria != null)
            {
                return Registry.Instance.CertificateRepository.GetCertificate(
                    certFindCriteria.ClientCertificateFindType,
                    certFindCriteria.ClientCertificateFindValue);
            }

            var embeddedCertInfo = configuration.ClientCertificateInformation as PrivateKeyCertificate;

            if (embeddedCertInfo != null)
            {
                return new X509Certificate2(Convert.FromBase64String(embeddedCertInfo.Certificate), embeddedCertInfo.Password, X509KeyStorageFlags.Exportable);
            }

            throw new NotSupportedException("The TLS certificate information specified in the PMode could not be used to retrieve the certificate");
        }

        private static async Task<bool> TryWriteToHttpRequestStreamAsync(HttpWebRequest request, MessagingContext messagingContext)
        {
            try
            {
                SetAdditionalRequestHeaders(request, messagingContext);

                using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                {
                    if (messagingContext.ReceivedMessage != null)
                    {
                        await messagingContext.ReceivedMessage.UnderlyingStream.CopyToFastAsync(requestStream).ConfigureAwait(false);
                    }
                    else
                    {
                        // Serialize the AS4 Message to the request-stream
                        var serializer = SerializerProvider.Default.Get(request.ContentType);
                        serializer.Serialize(messagingContext.AS4Message, requestStream, CancellationToken.None);
                    }
                }

                return true;
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.ConnectFailure && (messagingContext.AS4Message?.IsPullRequest ?? false))
                {
                    Logger.Trace($"The PullRequest could not be send to {request.RequestUri}");
                    Logger.Trace(exception.Message);
                    return false;
                }

                Logger.Error(exception.Message);
                if (exception.InnerException != null)
                {
                    Logger.Error(exception.InnerException.Message);
                }
                throw CreateFailedSendException(request.RequestUri.ToString(), exception);
            }
        }

        private static void SetAdditionalRequestHeaders(HttpWebRequest request, MessagingContext messagingContext)
        {
            long messageSize = TryGetMessageSize(messagingContext);

            if (messageSize >= 0)
            {
                request.ContentLength = messageSize;
            }

            request.AllowWriteStreamBuffering = false;
        }

        private static async Task<AS4Message> GetAS4MessageFromContextAsync(MessagingContext context)
        {
            if (context.ReceivedMessage != null)
            {
                return await GetAS4MessageFromStreamAsync(context.ReceivedMessage.UnderlyingStream, context.ReceivedMessage.ContentType).ConfigureAwait(false);
            }
            else
            {
                return context.AS4Message;
            }
        }

        private static async Task<AS4Message> GetAS4MessageFromStreamAsync(Stream stream, string contentType)
        {
            var serializer = SerializerProvider.Default.Get(contentType);

            stream.Position = 0;
            var as4Message = await serializer.DeserializeAsync(stream, contentType, CancellationToken.None).ConfigureAwait(false);
            stream.Position = 0;

            return as4Message;
        }

        private static long TryGetMessageSize(MessagingContext messagingContext)
        {
            if (messagingContext.ReceivedMessage?.UnderlyingStream?.CanSeek ?? false)
            {
                return messagingContext.ReceivedMessage.UnderlyingStream.Length;
            }

            if (messagingContext.AS4Message != null)
            {
                return messagingContext.AS4Message.DetermineMessageSize(SerializerProvider.Default);
            }

            return 0L;
        }

        private async Task<StepResult> TryHandleHttpResponseAsync(
            HttpWebRequest request,
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            Logger.Debug($"AS4 Message received from: {request.Address}");

            (HttpWebResponse webResponse, WebException exception) response = await _httpClient.Respond(request).ConfigureAwait(false);

            if (response.webResponse != null
                && ContentTypeSupporter.IsContentTypeSupported(response.webResponse.ContentType))
            {
                return
                    await HandleAS4Response(messagingContext, response.webResponse, cancellationToken)
                        .ConfigureAwait(false);
            }

            throw CreateFailedSendException(request.RequestUri.ToString(), response.exception);
        }

        private async Task UpdateMessageStatusAsync(AS4Message as4Message, Operation operation, OutStatus status)
        {
            if (as4Message == null)
            {
                return;
            }

            using (DatastoreContext context = _createDatastore())
            {
                var repository = new DatastoreRepository(context);

                repository.UpdateOutMessages(
                    outMessage => as4Message.MessageIds.Contains(outMessage.EbmsMessageId),
                    outMessage =>
                    {
                        outMessage.SetOperation(operation);
                        outMessage.SetStatus(status);
                    });

                var receptionAwareness = repository.GetReceptionAwareness(as4Message.MessageIds);

                UpdateReceptionAwareness(receptionAwareness);

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static void UpdateReceptionAwareness(IEnumerable<Entities.ReceptionAwareness> receptionAwarenessItems)
        {
            foreach (var item in receptionAwarenessItems)
            {
                item.LastSendTime = DateTimeOffset.Now;
                item.CurrentRetryCount += 1;
            }
        }

        private static async Task<StepResult> HandleAS4Response(
            MessagingContext originalMessage,
            WebResponse webResponse,
            CancellationToken cancellation)
        {
            using (AS4Response as4Response =
                await AS4Response.Create(originalMessage, webResponse as HttpWebResponse, cancellation).ConfigureAwait(false))
            {
                var responseHandler = new EmptyBodyResponseHandler(new PullRequestResponseHandler(new TailResponseHandler()));
                return await responseHandler.HandleResponse(as4Response).ConfigureAwait(false);
            }
        }

        private static WebException CreateFailedSendException(string requestUrl, Exception exception)
        {
            string description = $"Failed to Send AS4 Message to Url: {requestUrl}.";

            return new WebException(description, exception);
        }
    }
}