using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class InboundExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;
        private readonly IConfig _configuration;
        private readonly IAS4MessageBodyStore _bodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHandler"/> class.
        /// </summary>
        public InboundExceptionHandler() 
            : this(
                Registry.Instance.CreateDatastoreContext,
                Config.Instance,
                Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHandler"/> class.
        /// </summary>
        public InboundExceptionHandler(
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
        /// <param name="messageToTransform">The <see cref="ReceivsedMessage"/> that must be transformed by the transformer.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform)
        {
            Logger.Error(exception.Message);
            Logger.Trace(exception.StackTrace);

            await UseRepositorySaveAfterwards(
                async service => await service.InsertIncomingExceptionAsync(exception, messageToTransform.UnderlyingStream));

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
            Logger.Error(exception.Message);
            Logger.Trace(exception.StackTrace);

            return await HandleExecutionException(exception, context);
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            Logger.Error(exception.Message);

            await UseRepositorySaveAfterwards(async service =>
            {
                if (context.SubmitMessage != null)
                {
                    await service.InsertIncomingSubmitExceptionAsync(exception, context.SubmitMessage, context.ReceivingPMode);
                }
                else
                {
                    await service.InsertIncomingAS4MessageExceptionAsync(exception, context.EbmsMessageId, context.ReceivingPMode);
                }
            });

            return new MessagingContext(exception)
            {
                ErrorResult = context.ErrorResult
            };
        }

        private async Task UseRepositorySaveAfterwards(Func<ExceptionService, Task> usage)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new ExceptionService(Config.Instance, repository, Registry.Instance.MessageBodyStore);

                await usage(service);

                await context.SaveChangesAsync();
            }
        }
    }
}