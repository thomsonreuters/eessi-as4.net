using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Singletons;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Create an <see cref="AS4Message"/> from a <see cref="SubmitMessage"/>
    /// </summary>
    public class CreateAS4MessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private InternalMessage _internalMessage;

        /// <summary>
        /// Start Mapping from a <see cref="SubmitMessage"/> 
        /// to an <see cref="AS4Message"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Thrown when creating an <see cref="AS4Message"/> Fails (Mapping, Building...)</exception>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                _internalMessage = internalMessage;
                _internalMessage.AS4Message = CreateAS4Message(internalMessage);

                return await StepResult.SuccessAsync(internalMessage);
            }
            catch (Exception exception)
            {
                throw ThrowNewAS4Exception(exception, internalMessage);
            }
        }

        private static AS4Exception ThrowNewAS4Exception(Exception innerException, InternalMessage internalMessage)
        {
            string generatedMessageId = IdentifierFactory.Instance.Create();
            string description = $"[generated: {generatedMessageId}] Unable to Create AS4 Message from Submit Message";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithSendingPMode(internalMessage?.SendingPMode)
                .WithMessageIds(generatedMessageId)
                .WithInnerException(innerException)
                .Build();
        }

        private static AS4Message CreateAS4Message(InternalMessage internalMessage)
        {
            UserMessage userMessage = CreateUserMessage(internalMessage);
            Logger.Info($"[{userMessage.MessageId}] Create AS4Message with Submit Message");

            return new AS4MessageBuilder()
                .WithSendingPMode(internalMessage.SubmitMessage.PMode)
                .WithUserMessage(userMessage)
                .Build();
        }

        private static UserMessage CreateUserMessage(InternalMessage internalMessage)
        {
            Logger.Debug("Map Submit Message to UserMessage");
            
            return AS4Mapper.Map<UserMessage>(internalMessage.SubmitMessage);
        }
    }
}
