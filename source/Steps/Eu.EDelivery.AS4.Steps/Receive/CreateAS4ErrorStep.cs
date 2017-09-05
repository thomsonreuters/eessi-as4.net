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
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using NLog;
using Error = Eu.EDelivery.AS4.Model.Core.Error;
using PullRequest = Eu.EDelivery.AS4.Model.Core.PullRequest;
using SignalMessage = Eu.EDelivery.AS4.Model.Core.SignalMessage;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class CreateAS4ErrorStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createDatastoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class.
        /// </summary>
        public CreateAS4ErrorStep() : this(Registry.Instance.CreateDatastoreContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class.
        /// </summary>
        public CreateAS4ErrorStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
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
            if ((messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty) && messagingContext.ErrorResult == null)
            {
                return await StepResult.SuccessAsync(messagingContext);
            }

            Logger.Info($"[{messagingContext.EbmsMessageId}] Create AS4 Error Message");

            AS4Message errorMessage = CreateAS4Error(messagingContext);

            if (messagingContext.ErrorResult != null)
            {
                await CreateExceptionForReceivedSignalMessages(messagingContext);
            }

            messagingContext.ModifyContext(errorMessage);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task CreateExceptionForReceivedSignalMessages(MessagingContext context)
        {
            var signalMessages = context.AS4Message.SignalMessages;

            if (signalMessages.Any() == false)
            {
                return;
            }

            using (var dbContext = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(dbContext);

                var pmodeString = context.GetReceivingPModeString();

                foreach (var signal in signalMessages)
                {
                    if (signal is PullRequest)
                    {
                        continue;
                    }

                    var inException = CreateInException(signal, context.ErrorResult);
                    inException.PMode = pmodeString;

                    repository.InsertInException(inException);
                }

                repository.UpdateInMessages(m => signalMessages.Select(s => s.MessageId).Contains(m.EbmsMessageId),
                                            m => m.Status = InStatus.Exception);

                await dbContext.SaveChangesAsync();
            }
        }

        private static InException CreateInException(SignalMessage message, ErrorResult error)
        {
            var inException = new InException
            {
                EbmsRefToMessageId = message.MessageId,
                Exception = error.Description,                
                Operation = Operation.NotApplicable
            };

            return inException;
        }

        private static AS4Message CreateAS4Error(MessagingContext context)
        {
            AS4Message errorMessage = AS4Message.Create(context.SendingPMode);
            errorMessage.SigningId = context.AS4Message.SigningId;

            foreach (UserMessage userMessage in context.AS4Message.UserMessages)
            {
                Error error = CreateError(userMessage.MessageId, context);
                errorMessage.MessageUnits.Add(error);
            }

            return errorMessage;
        }

        private static Error CreateError(string userMessageId, MessagingContext originalContext)
        {
            Error error = new ErrorBuilder()
                .WithRefToEbmsMessageId(userMessageId)
                .WithErrorResult(originalContext.ErrorResult)
                .Build();

            if (originalContext.SendingPMode?.MessagePackaging?.IsMultiHop == true)
            {
                error.MultiHopRouting =
                    AS4Mapper.Map<RoutingInputUserMessage>(originalContext.AS4Message?.PrimaryUserMessage);
            }

            return error;
        }
    }
}