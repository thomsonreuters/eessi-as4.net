using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
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


        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4ErrorStep"/> class.
        /// </summary>
        public CreateAS4ErrorStep() { }

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

            Logger.Info($"[{messagingContext.AS4Message?.GetPrimaryMessageId()}] Create AS4 Error Message");

            AS4Message errorMessage = CreateAS4Error(messagingContext);
            messagingContext.ModifyContext(errorMessage);
          
            return await StepResult.SuccessAsync(messagingContext);
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