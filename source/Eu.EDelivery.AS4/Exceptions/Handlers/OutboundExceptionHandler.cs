using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class OutboundExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;
        private readonly IConfig _configuration;
        private readonly IAS4MessageBodyStore _bodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundExceptionHandler"/> class.
        /// </summary>
        public OutboundExceptionHandler() 
            : this(
                Registry.Instance.CreateDatastoreContext,
                Config.Instance,
                Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="configuration"></param>
        /// <param name="bodyStore"></param>
        public OutboundExceptionHandler(
            Func<DatastoreContext> createContext,
            IConfig configuration,
            IAS4MessageBodyStore bodyStore)
        {
            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (bodyStore == null)
            {
                throw new ArgumentNullException(nameof(bodyStore));
            }

            _createContext = createContext;
            _configuration = configuration;
            _bodyStore = bodyStore;
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
                async service => await service.InsertOutgoingExceptionAsync(exception, messageToTransform.UnderlyingStream));

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

            string ebmsMessageId = await GetEbmsMessageId(context);
            await UseRepositorySaveAfterwards(async service =>
            {
                if (context.SubmitMessage != null)
                {
                    await service.InsertOutgoingSubmitExceptionAsync(exception, context.SubmitMessage, context.SendingPMode);
                }
                else
                {
                    await service.InsertOutgoingAS4MessageExceptionAsync(exception, ebmsMessageId, context.MessageEntityId, context.SendingPMode);
                }
            });

            if (context.SubmitMessage == null)
            {
                await UseRepositorySaveAfterwards(service =>
                {
                    service.InsertRelatedRetryForOutException(
                        ebmsMessageId,
                        context.SendingPMode?.ExceptionHandling?.Reliability);

                    return Task.CompletedTask;
                });
            }

            return new MessagingContext(exception);
        }

        private async Task UseRepositorySaveAfterwards(Func<ExceptionService, Task> usage)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new ExceptionService(_configuration, repository, _bodyStore);

                await usage(service);
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

