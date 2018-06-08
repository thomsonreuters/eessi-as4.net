using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;
using NLog;
using RetryReliability = Eu.EDelivery.AS4.Model.PMode.RetryReliability;

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

            await UseRepositorySaveAfterwards(
                repo => repo.InsertInException(
                    new InException(
                        messageToTransform.UnderlyingStream.ToBytes(), 
                        exception)));

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

            InException ex = await CreateInExceptionBasedOnContext(exception, context);
            await UseRepositorySaveAfterwards(repo =>
            {
                if (!isSubmitMessage)
                {
                    repo.UpdateInMessage(
                        context.EbmsMessageId, 
                        m => m.SetStatus(InStatus.Exception));
                }

                repo.InsertInException(ex);
            });

            Entities.RetryReliability r = CreateRelatedRetryForInException(
                ex.Id,
                context.ReceivingPMode?.ExceptionHandling?.Reliability);

            if (r != null)
            {
                await UseRepositorySaveAfterwards(repo => repo.InsertRetryReliability(r));
            }

            return new MessagingContext(exception)
            {
                ErrorResult = context.ErrorResult
            };
        }

        private static async Task<InException> CreateInExceptionBasedOnContext(Exception exception, MessagingContext context)
        {
            async Task<InException> CreateInException()
            {
                if (context.SubmitMessage != null)
                {
                    return new InException(
                        await AS4XmlSerializer.TryToXmlBytesAsync(context.SubmitMessage),
                        exception);
                }

                return new InException(context.EbmsMessageId, exception);
            }

            InException inEx = await CreateInException();
            await inEx.SetPModeInformationAsync(context.ReceivingPMode);

            ReceiveHandling handling = context.ReceivingPMode?.ExceptionHandling;
            bool needsToBeNotified = handling?.NotifyMessageConsumer == true;
            inEx.SetOperation(needsToBeNotified ? Operation.ToBeNotified : default(Operation));

            return inEx;
        }

        private static Entities.RetryReliability CreateRelatedRetryForInException(long refToInExceptionId, RetryReliability reliability)
        {
            if (reliability != null && reliability.IsEnabled)
            {
                return Entities.RetryReliability.CreateForInException(
                    refToInExceptionId: refToInExceptionId,
                    maxRetryCount: reliability.RetryCount,
                    retryInterval: reliability.RetryInterval.AsTimeSpan(),
                    type: RetryType.Notification);
            }

            return null;
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
    }
}