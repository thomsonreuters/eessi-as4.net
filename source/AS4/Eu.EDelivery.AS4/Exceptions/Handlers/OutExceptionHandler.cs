using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class OutExceptionHandler : IAgentExceptionHandler
    {
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public OutExceptionHandler(Func<DatastoreContext> createContext)
        {
            _createContext = createContext;
        }

        /// <summary>
        /// Handles the transformation exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleTransformationException(Exception exception)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var outException = new OutException {EbmsRefToMessageId = Guid.NewGuid().ToString(), Exception = exception.Message};

                repository.InsertOutException(outException);
                await context.SaveChangesAsync();
            }

            return null;
        }

        /// <summary>
        /// Handles the execution exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleExecutionException(Exception exception, MessagingContext context)
        {
            throw new NotImplementedException();
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
