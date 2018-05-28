using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class OutboundExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundExceptionHandler"/> class.
        /// </summary>
        public OutboundExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public OutboundExceptionHandler(Func<DatastoreContext> createContext)
        {
            _createContext = createContext;
        }

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageToTransform">The <see cref="ReceivedMessage"/> that must be transformed by the transformer.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform)
        {
            Logger.Error($"Exception occured during transformation: {exception.Message}");

            await UseRepositorySaveAfterwards(
                repository =>
                {
                    var outException = new OutException(messageToTransform.UnderlyingStream.ToBytes(), exception);
                    repository.InsertOutException(outException);
                });

            return new MessagingContext(exception);
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            Logger.Error($"Exception occured while executing Error Pipeline: {exception.Message}");
            return await HandleExecutionException(exception, context);
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The message context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            Logger.Error($"Exception occured while executing Steps: {exception.Message}");
            Logger.Trace(exception.StackTrace);

            if (exception.InnerException != null)
            {
                Logger.Error(exception.InnerException.Message);
                Logger.Trace(exception.InnerException.StackTrace);
            }

            OutException outException = await CreateOutExceptionBasedOnContext(exception, context);
            await UseRepositorySaveAfterwards(repo =>
            {
                if (context.MessageEntityId != null)
                {
                    repo.UpdateOutMessage(
                        context.MessageEntityId.Value,
                        m => m.SetStatus(OutStatus.Exception));
                }

                repo.InsertOutException(outException);
            });

            return new MessagingContext(exception);
        }

        private static async Task<OutException> CreateOutExceptionBasedOnContext(Exception exception, MessagingContext context)
        {
            async Task<OutException> CreateOutException()
            {
                string ebmsMessageId = await GetEbmsMessageId(context);
                if (string.IsNullOrEmpty(ebmsMessageId))
                {
                    return new OutException(
                        await AS4XmlSerializer.TryToXmlBytesAsync(context.SubmitMessage),
                        exception);
                }

                return new OutException(ebmsMessageId, exception);
            }

            OutException outEx = await CreateOutException();
            await outEx.SetPModeInformationAsync(context.SendingPMode);

            SendHandling handling = context.SendingPMode?.ExceptionHandling;

            bool needsToBeNotified = handling?.NotifyMessageProducer == true;
            outEx.SetOperation(needsToBeNotified ? Operation.ToBeNotified : default(Operation));

            RetryReliability reliability = handling?.Reliability;
            if (reliability != null && reliability.IsEnabled)
            {
                outEx.CurrentRetryCount = 0;
                outEx.MaxRetryCount = reliability.RetryCount;
                outEx.SetRetryInterval(reliability.RetryInterval.AsTimeSpan());
            }

            return outEx;
        }

        private async Task UseRepositorySaveAfterwards(Action<DatastoreRepository> usage)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                usage(repository);

                await context.SaveChangesAsync();
            }
        }

        private static async Task<string> GetEbmsMessageId(MessagingContext context)
        {
            string ebmsMessageId = context.EbmsMessageId;

            if (String.IsNullOrWhiteSpace(ebmsMessageId) && context.ReceivedMessage != null)
            {
                AS4Message as4Message = await TryDeserialize(context.ReceivedMessage);
                ebmsMessageId = as4Message?.GetPrimaryMessageId();
            }

            return ebmsMessageId;
        }

        private static async Task<AS4Message> TryDeserialize(ReceivedMessage message)
        {
            ISerializer serializer = SerializerProvider.Default.Get(message.ContentType);
            try
            {
                message.UnderlyingStream.Position = 0;

                return await serializer.DeserializeAsync(
                    message.UnderlyingStream, 
                    message.ContentType, 
                    CancellationToken.None);
            }
            catch
            {
                return null;
            }
        }
    }
}

