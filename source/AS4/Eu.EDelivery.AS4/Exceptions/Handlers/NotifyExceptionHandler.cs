using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    internal class NotifyExceptionHandler : IAgentExceptionHandler
    {
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyExceptionHandler"/> class.
        /// </summary>
        public NotifyExceptionHandler() : this(Registry.Instance.CreateDatastoreContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyExceptionHandler"/> class.
        /// </summary>
        public NotifyExceptionHandler(Func<DatastoreContext> createContext)
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
            var entity = GetReceivedEntity(messageToTransform);


            if (entity is InMessage || entity is InException)
            {
                var inboundHandler = new InboundExceptionHandler();
                return await inboundHandler.HandleTransformationException(exception, messageToTransform).ConfigureAwait(false);
            }
            else
            {
                var outboundHandler = new OutboundExceptionHandler();
                return await outboundHandler.HandleTransformationException(exception, messageToTransform).ConfigureAwait(false);
            }


        }

        private static Entity GetReceivedEntity(ReceivedMessage message)
        {
            var receivedEntityMessage = message as ReceivedEntityMessage;

            if (receivedEntityMessage == null)
            {
                throw new InvalidOperationException("A ReceivedEntityMessage is expected in the NotifyExceptionHandler.HandleTransformationException method");
            }

            return receivedEntityMessage.Entity;
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            return await HandleNotifyException(exception, context);
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            return await HandleNotifyException(exception, context);
        }

        private async Task<MessagingContext> HandleNotifyException(Exception exception, MessagingContext context)
        {
            using (var dbContext = _createContext())
            {
                var repository = new DatastoreRepository(dbContext);

                if (context.NotifyMessage.EntityType == typeof(InMessage) ||
                    context.NotifyMessage.EntityType == typeof(InException))
                {
                    var inException = CreateMinimumExceptionEntity<InException>(exception, context);
                    repository.InsertInException(inException);

                    if (context.NotifyMessage.EntityType == typeof(InMessage))
                    {
                        repository.UpdateInMessage(context.EbmsMessageId, i => i.Status = InStatus.Exception);
                    }
                }
                else if (context.NotifyMessage.EntityType != typeof(OutMessage) ||
                         context.NotifyMessage.EntityType == typeof(OutException))
                {
                    var outException = CreateMinimumExceptionEntity<OutException>(exception, context);
                    repository.InsertOutException(outException);

                    if (context.NotifyMessage.EntityType == typeof(OutMessage))
                    {
                        repository.UpdateOutMessage(context.EbmsMessageId, o => o.Status = OutStatus.Exception);
                    }
                }

                await dbContext.SaveChangesAsync();
            }

            return new MessagingContext(exception);
        }

        private static T CreateMinimumExceptionEntity<T>(Exception exception, MessagingContext context) where T : ExceptionEntity, new()
        {
            return new T
            {
                EbmsRefToMessageId = context.EbmsMessageId,
                Exception = exception.ToString()
            };
        }
    }
}
