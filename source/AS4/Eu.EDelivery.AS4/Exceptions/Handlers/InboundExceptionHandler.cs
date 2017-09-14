using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using NLog;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class InboundExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHandler"/> class.
        /// </summary>
        public InboundExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public InboundExceptionHandler(Func<DatastoreContext> createContext)
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
            Logger.Error(exception.Message);
            Logger.Trace(exception.StackTrace);

            await SideEffectRepositoryUsage(
                repository =>
                {
                    InException ex = new InException(messageToTransform.UnderlyingStream.ToBytes(), exception);
                    repository.InsertInException(ex);

                    return Task.CompletedTask;
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
            bool isSubmitMessage = context.SubmitMessage != null;

            if (isSubmitMessage)
            {
                await SideEffectRepositoryUsage(
                    async repository =>
                    {                        
                        InException ex = new InException(await AS4XmlSerializer.TryToXmlBytesAsync(context.SubmitMessage), exception);
                        await DecorateExceptionEntityWithContextInfoAsync(ex, context);

                        repository.InsertInException(ex);
                    });
            }
            else
            {
                await SideEffectRepositoryUsage(
                    async repository =>
                    {
                        repository.UpdateInMessage(context.EbmsMessageId, m => m.SetStatus(InStatus.Exception));

                        InException ex = new InException(context.EbmsMessageId, exception);
                        await DecorateExceptionEntityWithContextInfoAsync(ex, context);
                        
                        repository.InsertInException(ex);
                    });
            }

            return CreateExceptionContext(exception, context);
        }

        private static MessagingContext CreateExceptionContext(Exception exception, MessagingContext context)
        {
            var exceptionContext = new MessagingContext(exception)
            {
                ErrorResult = context.ErrorResult
            };

            return exceptionContext;
        }

        private static async Task DecorateExceptionEntityWithContextInfoAsync(InException exceptionEntity, MessagingContext context)
        {
            if (context != null)
            {
                Operation notifyOperation =
                    context.ReceivingPMode?.ExceptionHandling?.NotifyMessageConsumer == true
                        ? Operation.ToBeNotified
                        : default(Operation);

                exceptionEntity.SetOperation(notifyOperation);
                await exceptionEntity.SetPModeInformationAsync(context.ReceivingPMode);
            }
        }
        
        private async Task SideEffectRepositoryUsage(Func<DatastoreRepository, Task> usage)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);

                await usage(repository);

                await context.SaveChangesAsync();
            }
        }
    }
}

