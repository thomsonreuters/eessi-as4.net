using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class InboundExceptionHanlder : IAgentExceptionHandler
    {
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHanlder"/> class.
        /// </summary>
        public InboundExceptionHanlder() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="InboundExceptionHanlder" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public InboundExceptionHanlder(Func<DatastoreContext> createContext)
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
            await InsertInException(exception, inException => inException.MessageBody = contents.ToArray());
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
            await InsertInException(
                exception,
                inException =>
                {
                    inException.EbmsRefToMessageId = context.AS4Message?.GetPrimaryMessageId();

                    inException.PMode = AS4XmlSerializer.ToString(context.ReceivingPMode);
                    inException.Operation = 
                        context.ReceivingPMode?.ExceptionHandling?.NotifyMessageConsumer == true
                            ? Operation.ToBeNotified
                            : default(Operation);
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
            throw new NotImplementedException();
        }

        private async Task InsertInException(Exception exception, Action<InException> alterException)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);

                var inException = new InException
                {
                    EbmsRefToMessageId = Guid.NewGuid().ToString(),
                    Exception = exception.Message,
                    InsertionTime = DateTimeOffset.Now,
                    ModificationTime = DateTimeOffset.Now
                };
                alterException(inException);

                repository.InsertInException(inException);
                await context.SaveChangesAsync();
            }
        }
    }
}

