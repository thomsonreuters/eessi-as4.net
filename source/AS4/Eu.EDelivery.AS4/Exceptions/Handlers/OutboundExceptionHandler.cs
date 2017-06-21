using System;
using System.IO;
using System.Text;
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
    public class OutboundExceptionHandler : IAgentExceptionHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundExceptionHandler"/> class.
        /// </summary>
        public OutboundExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) {}

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
        /// <param name="contents">The contents.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception, Stream contents)
        {
            Logger.Error(exception.Message);
            await InsertOutException(exception, outException => outException.MessageBody = contents.ToArray());

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
            return await HandleExecutionException(exception, context);
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageContext">The message context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext messageContext)
        {
            Logger.Error(exception.Message);

            string messageId = messageContext.EbmsMessageId;
            Action<OutException> updateException = ex => { };

            if (string.IsNullOrEmpty(messageId) == false)
            {
                await SideEffectUsageRepository(
                    repository => repository.UpdateOutMessage(messageId, m => m.Status = OutStatus.Exception));
                updateException = ex => ex.EbmsRefToMessageId = messageId;
            }
            else
            {
                updateException = ex => ex.MessageBody = AS4XmlSerializer.TryToXmlBytesAsync(messageContext.SubmitMessage).Result;
            }

            await InsertOutException(exception, messageContext, updateException);
            return new MessagingContext(exception);
        }

        private async Task InsertOutException(Exception exception, MessagingContext messageContext, Action<OutException> updateException)
        {
            await InsertOutException(
                   exception,
                   outException =>
                   {
                       updateException(outException);

                       outException.PMode = AS4XmlSerializer.ToString(messageContext.SendingPMode);
                       outException.Operation =
                           messageContext.SendingPMode?.ExceptionHandling?.NotifyMessageProducer == true
                               ? Operation.ToBeNotified
                               : default(Operation);
                   });
        }

        private async Task InsertOutException(Exception exception, Action<OutException> alterException)
        {
            await SideEffectUsageRepository(
                repository =>
                {
                    var outException = new OutException
                    {
                        EbmsRefToMessageId = Guid.NewGuid().ToString(),
                        Exception = exception.Message,
                        InsertionTime = DateTimeOffset.Now,
                        ModificationTime = DateTimeOffset.Now
                    };
                    alterException(outException);

                    repository.InsertOutException(outException);
                });
        }

        private async Task SideEffectUsageRepository(Action<DatastoreRepository> usage)
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
