using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Create an <see cref="Error"/> 
    /// from a given <see cref="AS4Exception"/>
    /// </summary>
    public class CreateAS4ErrorStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new intance of the type <see cref="CreateAS4ErrorStep"/> class
        /// </summary>
        public CreateAS4ErrorStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start creating <see cref="Error"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            if (ShouldCreateError(internalMessage) == false)
            {
                return await StepResult.SuccessAsync(internalMessage);
            }

            var errorMessage = CreateAS4ErrorMessage(internalMessage);

            // Save the Error Message as well .... 
            using (var db = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(db);
                var service = new OutMessageService(repository);
                await service.InsertErrorAsync(errorMessage);
            }

            return await StepResult.SuccessAsync(new InternalMessage(errorMessage));
        }

        private AS4Message CreateAS4ErrorMessage(InternalMessage internalMessage)
        {
            this._logger.Info($"{internalMessage.Prefix} Create AS4 Error Message from AS4 Exception");

            var builder = new AS4MessageBuilder();

            // Create an Error for every UserMessage that exists in the AS4Message
            foreach (var userMessage in internalMessage.AS4Message.UserMessages)
            {
                var error = CreateError(internalMessage.Exception, userMessage.MessageId, internalMessage.AS4Message);

                builder.WithSignalMessage(error);
            }

            if (internalMessage.AS4Message.ReceivingPMode != null)
            {
                builder.WithReceivingPMode(internalMessage.AS4Message.ReceivingPMode);
            }

            var errorMessage = builder.Build();

            errorMessage.SigningId = internalMessage.AS4Message.SigningId;
            errorMessage.SendingPMode = internalMessage.AS4Message.SendingPMode;

            return errorMessage;

        }

        private static bool ShouldCreateError(InternalMessage internalMessage)
        {
            return internalMessage.Exception != null && (internalMessage.AS4Message?.UserMessages?.Any() ?? false);
        }

        private static Error CreateError(AS4Exception exception, string userMessageId, AS4Message originalAS4Message)
        {
            return new ErrorBuilder()
                .WithRefToEbmsMessageId(userMessageId)
                .WithOriginalAS4Message(originalAS4Message)
                .WithAS4Exception(exception)
                .Build();
        }

    }
}