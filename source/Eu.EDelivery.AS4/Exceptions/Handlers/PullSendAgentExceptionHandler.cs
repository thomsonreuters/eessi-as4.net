using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class PullSendAgentExceptionHandler : IAgentExceptionHandler
    {
        private readonly Func<DatastoreContext> _createContext;

        private readonly OutboundExceptionHandler _outboundExceptionHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullSendAgentExceptionHandler"/> class.
        /// </summary>
        public PullSendAgentExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullSendAgentExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public PullSendAgentExceptionHandler(Func<DatastoreContext> createContext)
        {
            _createContext = createContext;
            _outboundExceptionHandler = new OutboundExceptionHandler(createContext);
        }

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageToTransform">The <see cref="ReceivedMessage"/> that must be transformed by the transformer.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, ReceivedMessage messageToTransform)
        {
            await InsertOutException(exception, messageToTransform.UnderlyingStream.ToBytes());

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

        private async Task InsertOutException(Exception exception, byte[] body)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var outException = new OutException(body, exception.Message);
                
                repository.InsertOutException(outException);
                await context.SaveChangesAsync();
            }
        }       
    }
}
