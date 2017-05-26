using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Create an <see cref="Error"/> 
    /// from a given <see cref="AS4Exception"/>
    /// </summary>
    public class CreateAS4ErrorStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAS4MessageBodyPersister _as4MessageBodyPersister;
        private readonly Func<DatastoreContext> _createDatastore;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class.
        /// </summary>
        public CreateAS4ErrorStep()
            : this(Config.Instance.OutgoingAS4MessageBodyPersister, Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class
        /// </summary>
        /// <param name="as4MessageBodyPersister">The <see cref="IAS4MessageBodyPersister"/> that must be used to persist the MessageBody.</param>
        /// <param name="createDatastoreContext">The context in which teh datastore context is set.</param>
        public CreateAS4ErrorStep(IAS4MessageBodyPersister as4MessageBodyPersister, Func<DatastoreContext> createDatastoreContext)
        {
            _as4MessageBodyPersister = as4MessageBodyPersister;
            _createDatastore = createDatastoreContext;
        }

        /// <summary>
        /// Start creating <see cref="Error"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (ShouldCreateError(internalMessage) == false)
            {
                return await StepResult.SuccessAsync(internalMessage);
            }

            AS4Message errorMessage = CreateAS4ErrorMessage(internalMessage);

            var message = new InternalMessage(errorMessage)
            {
                SendingPMode = internalMessage.SendingPMode,
                ReceivingPMode = internalMessage.ReceivingPMode
            };

            // Save the Error Message as well .... 
            using (DatastoreContext db = _createDatastore())
            {
                var service = new OutMessageService(new DatastoreRepository(db), _as4MessageBodyPersister);

                // The service will determine the correct operation for each message-part.
                await service.InsertAS4Message(message, Operation.NotApplicable, cancellationToken);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            
            return await StepResult.SuccessAsync(message);
        }

        private static AS4Message CreateAS4ErrorMessage(InternalMessage internalMessage)
        {
            Logger.Info($"{internalMessage.Prefix} Create AS4 Error Message from AS4 Exception");

            var builder = new AS4MessageBuilder();

            CreateErrorForEveryUserMessageIn(internalMessage, error => builder.WithSignalMessage(error));

            AS4Message errorMessage = builder.Build();

            errorMessage.SigningId = internalMessage.AS4Message.SigningId;

            return errorMessage;
        }

        private static void CreateErrorForEveryUserMessageIn(InternalMessage internalMessage, Action<Error> callback)
        {
            foreach (UserMessage userMessage in internalMessage.AS4Message.UserMessages)
            {
                Error error = CreateError(internalMessage.Exception, userMessage.MessageId, internalMessage);

                callback(error);
            }
        }

        private static bool ShouldCreateError(InternalMessage internalMessage)
        {
            return internalMessage.Exception != null && (internalMessage.AS4Message?.UserMessages?.Any() ?? false);
        }

        private static Error CreateError(AS4Exception exception, string userMessageId, InternalMessage originalAS4Message)
        {
            return new ErrorBuilder()
                .WithRefToEbmsMessageId(userMessageId)
                .WithOriginalMessage(originalAS4Message)
                .WithAS4Exception(exception)
                .Build();
        }

    }
}