using System;
using System.IO;
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
        public InboundExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) {}

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
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, Stream contents)
        {
            Logger.Error(exception.Message);

            await SideEffectRepositoryUsage(
                repository =>
                {
                    InException ex = CreateMinimumInException(exception);
                    ex.MessageBody = contents.ToBytes();
                    repository.InsertInException(ex);
                });

            return new MessagingContext(exception);
        }

        private static InException CreateMinimumInException(Exception exception)
        {
            return new InException
            {
                Exception = exception.Message,
                InsertionTime = DateTimeOffset.Now,
                ModificationTime = DateTimeOffset.Now
            };
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
                    repository =>
                    {
                        InException ex = CreateInExceptionWithContextInfo(exception, context);
                        ex.MessageBody = AS4XmlSerializer.TryToXmlBytesAsync(context.SubmitMessage).Result;

                        repository.InsertInException(ex);
                    });
            }
            else
            {
                await SideEffectRepositoryUsage(
                    repository =>
                    {
                        repository.UpdateInMessage(context.EbmsMessageId, m => m.Status = InStatus.Exception);

                        InException ex = CreateInExceptionWithContextInfo(exception, context);
                        ex.EbmsRefToMessageId = context.EbmsMessageId;

                        repository.InsertInException(ex);
                    });
            }
           
            return new MessagingContext(exception);
        }

        private static InException CreateInExceptionWithContextInfo(Exception exception, MessagingContext context)
        {
            InException inException = CreateMinimumInException(exception);

            if (context != null)
            {
                inException.PMode = AS4XmlSerializer.ToString(context.ReceivingPMode);
                inException.Operation = 
                    context.ReceivingPMode?.ExceptionHandling?.NotifyMessageConsumer == true
                        ? Operation.ToBeNotified
                        : default(Operation);
            }

            return inException;
        }

        private async Task SideEffectRepositoryUsage(Action<DatastoreRepository> usage)
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

