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

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyExceptionHandler"/> class.
        /// </summary>
        public NotifyExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyExceptionHandler"/> class.
        /// </summary>
        public NotifyExceptionHandler(Func<DatastoreContext> createContext)
        {
            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

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
                throw new InvalidOperationException($"A ReceivedEntityMessage is expected in the NotifyExceptionHandler.HandleTransformationException method instead of a {message.GetType().FullName}");
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
            Logger.Error(exception.Message);

            using (var dbContext = _createContext())
            {
                var repository = new DatastoreRepository(dbContext);

                if (context.NotifyMessage.EntityType == typeof(InMessage) ||
                    context.NotifyMessage.EntityType == typeof(InException))
                {
                    var inException = InException.ForEbmsMessageId(context.EbmsMessageId, exception);
                    repository.InsertInException(inException);

                    if (context.NotifyMessage.EntityType == typeof(InMessage))
                    {
                        Logger.Debug("Fatal fail in notification, set InMessage(s).Status=Exception");
                        repository.UpdateInMessage(context.EbmsMessageId, i => i.SetStatus(InStatus.Exception));
                    }
                }
                else if (context.NotifyMessage.EntityType != typeof(OutMessage) ||
                         context.NotifyMessage.EntityType == typeof(OutException))
                {
                    var outException = OutException.ForEbmsMessageId(context.EbmsMessageId, exception);
                    repository.InsertOutException(outException);

                    if (context.NotifyMessage.EntityType == typeof(OutMessage) &&
                        context.MessageEntityId != null)
                    {
                        Logger.Debug("Fatal fail in notification, set OutMessage.Status=Exception");
                        repository.UpdateOutMessage(
                            context.MessageEntityId.Value,
                            o => o.SetStatus(OutStatus.Exception));
                    }
                }

                if (context.MessageEntityId != null)
                {
                    Logger.Debug("Abort retry operation due to fatal notification exception, set Status=Completed");
                    repository.UpdateRetryReliability(
                        context.MessageEntityId.Value,
                        r => r.Status = RetryStatus.Completed);
                }

                await dbContext.SaveChangesAsync();
            }

            return new MessagingContext(exception);
        }
    }
}
