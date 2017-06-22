using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using NLog;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class CreateAS4ErrorStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAS4MessageBodyStore _messageBodyStore;
        private readonly Func<DatastoreContext> _createDatastore;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class.
        /// </summary>
        public CreateAS4ErrorStep() : this(Registry.Instance.MessageBodyStore, Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class
        /// </summary>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore"/> that must be used to persist the MessageBody.</param>
        /// <param name="createDatastoreContext">The context in which teh datastore context is set.</param>
        public CreateAS4ErrorStep(IAS4MessageBodyStore messageBodyStore, Func<DatastoreContext> createDatastoreContext)
        {
            _messageBodyStore = messageBodyStore;
            _createDatastore = createDatastoreContext;
        }

        /// <summary>
        /// Start creating <see cref="Error"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception">A delegate callback throws an exception.</exception>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message.IsEmpty && messagingContext.ErrorResult == null)
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            Logger.Info($"[{messagingContext.AS4Message?.GetPrimaryMessageId()}] Create AS4 Error Message");

            AS4Message errorMessage = CreateAS4Error(messagingContext);
            MessagingContext message = messagingContext.CloneWith(errorMessage);

            // Save the Error Message as well .... 
            using (DatastoreContext db = _createDatastore())
            {
                var service = new OutMessageService(new DatastoreRepository(db), _messageBodyStore);

                // The service will determine the correct operation for each message-part.
                await service.InsertAS4Message(message, Operation.NotApplicable, cancellationToken);
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            
            return await StepResult.SuccessAsync(message);
        }

        private static AS4Message CreateAS4Error(MessagingContext context)
        {
            AS4Message errorMessage = AS4Message.Create(context.SendingPMode);
            errorMessage.SigningId = context.AS4Message.SigningId;

            foreach (UserMessage userMessage in context.AS4Message.UserMessages)
            {
                Error error = CreateError(userMessage.MessageId, context);
                errorMessage.SignalMessages.Add(error);
            }

            return errorMessage;
        }

        private static Error CreateError(string userMessageId, MessagingContext originalContex)
        {
            Error error = new ErrorBuilder()
                .WithRefToEbmsMessageId(userMessageId)
                .WithErrorResult(originalContex.ErrorResult)
                .Build();

            if (originalContex.SendingPMode?.MessagePackaging.IsMultiHop == true)
            {
                error.MultiHopRouting =
                    AS4Mapper.Map<RoutingInputUserMessage>(originalContex.AS4Message?.PrimaryUserMessage);
            }

            return error;
        }
    }
}