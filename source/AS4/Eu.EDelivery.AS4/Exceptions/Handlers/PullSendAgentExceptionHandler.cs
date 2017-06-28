using System;
using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class PullSendAgentExceptionHandler : IAgentExceptionHandler
    {
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullSendAgentExceptionHandler"/> class.
        /// </summary>
        public PullSendAgentExceptionHandler() : this(Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PullSendAgentExceptionHandler" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        public PullSendAgentExceptionHandler(Func<DatastoreContext> createContext)
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
            await InsertOutException(exception, contents.ToBytes());

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
            return await HandleErrorException(exception, context);
        }

        /// <summary>
        /// Handles the error exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task<MessagingContext> HandleErrorException(Exception exception, MessagingContext context)
        {
            byte[] body = await AS4XmlSerializer.ToSoapEnvelopeBytesAsync(context.AS4Message);
            await InsertOutException(exception, body);

            return new MessagingContext(BuildAS4Error(context), MessagingContextMode.Send);
        }

        private async Task InsertOutException(Exception exception, byte[] body)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                var outException = new OutException
                {
                    Exception = exception.Message,
                    MessageBody = body,
                    InsertionTime = DateTimeOffset.Now,
                    ModificationTime = DateTimeOffset.Now
                };

                repository.InsertOutException(outException);
                await context.SaveChangesAsync();
            }
        }

        private static AS4Message BuildAS4Error(MessagingContext messagingContext)
        {
            Error error = new ErrorBuilder()
                .WithRefToEbmsMessageId(messagingContext.AS4Message.GetPrimaryMessageId())
                .WithErrorResult(messagingContext.ErrorResult)
                .Build();

            return AS4Message.Create(error);
        }
    }
}
