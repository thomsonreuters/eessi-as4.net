using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class PullSendAgentExceptionHandler : IAgentExceptionHandler
    {
        private readonly Func<DatastoreContext> _createContext;
        private readonly IConfig _configuration;
        private readonly IAS4MessageBodyStore _bodyStore;
        private readonly OutboundExceptionHandler _outboundExceptionHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullSendAgentExceptionHandler"/> class.
        /// </summary>
        public PullSendAgentExceptionHandler() 
            : this(
                Registry.Instance.CreateDatastoreContext,
                Config.Instance,
                Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullSendAgentExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="configuration"></param>
        /// <param name="bodyStore"></param>
        public PullSendAgentExceptionHandler(
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
            _outboundExceptionHandler = new OutboundExceptionHandler(createContext, configuration, bodyStore);
        }

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageToTransform">The <see cref="ReceivedMessage"/> that must be transformed by the transformer.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform)
        {
            await InsertOutException(exception, messageToTransform.UnderlyingStream);

            return new MessagingContext(exception);
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            return await _outboundExceptionHandler.HandleExecutionException(exception, context);
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            return await _outboundExceptionHandler.HandleErrorException(exception, context);
        }

        private async Task InsertOutException(Exception exception, Stream body)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var service = new ExceptionService(_configuration, repository, _bodyStore);

                await service.InsertOutgoingExceptionAsync(exception, body);
                await context.SaveChangesAsync();
            }
        }       
    }
}
