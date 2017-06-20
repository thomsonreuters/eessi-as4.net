using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class OutboundExceptionHandler : IAgentExceptionHandler
    {
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
        /// <param name="contents">The contents.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Stream contents, Exception exception)
        {
            await InsertOutException(exception, outException => outException.MessageBody = contents.ToArray());

            return new MessagingContext(exception);
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="messageContext">The message context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext messageContext)
        {
            await InsertOutException(
                exception,
                outException =>
                {
                    outException.EbmsRefToMessageId = 
                        messageContext.AS4Message?.GetPrimaryMessageId();

                    outException.Operation = 
                        messageContext.SendingPMode?.ExceptionHandling?.NotifyMessageProducer == true
                            ? Operation.ToBeNotified
                            : default(Operation);
                });

            return new MessagingContext(exception);
        }

        private async Task InsertOutException(Exception exception, Action<OutException> alterException)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);

                var outException = new OutException
                {
                    EbmsRefToMessageId = Guid.NewGuid().ToString(),
                    Exception = exception.Message,
                    InsertionTime = DateTimeOffset.Now,
                    ModificationTime = DateTimeOffset.Now
                };
                alterException(outException);

                repository.InsertOutException(outException);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
