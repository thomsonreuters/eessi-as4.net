using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// Create an <see cref="AS4Message"/> from a <see cref="SubmitMessage"/>
    /// </summary>
    public class CreateAS4MessageStep : IStep
    {
        private readonly ILogger _logger;
        private readonly AS4MessageBuilder _builder;
        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4MessageStep"/> class
        /// </summary>
        public CreateAS4MessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
            this._builder = new AS4MessageBuilder();
        }

        /// <summary>
        /// Start Mapping from a <see cref="SubmitMessage"/> 
        /// to an <see cref="AS4Message"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Thrown when creating an <see cref="AS4Message"/> Fails (Mapping, Building...)</exception>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                this._internalMessage = internalMessage;
                this._internalMessage.AS4Message = CreateAS4Message();
                return StepResult.SuccessAsync(internalMessage);
            }
            catch (Exception exception)
            {
                throw ThrowNewAS4Exception(exception);
            }
        }

        private AS4Exception ThrowNewAS4Exception(Exception innerException)
        {
            string generatedMessageId = IdentifierFactory.Instance.Create();
            string description = $"[generated: {generatedMessageId}] Unable to Create AS4 Message from Submit Message";
            this._logger.Error(description);

            return new AS4ExceptionBuilder()
                .WithDescription(description)
                .WithSendingPMode(this._internalMessage.AS4Message.SendingPMode)
                .WithMessageIds(generatedMessageId)
                .WithInnerException(innerException)
                .Build();
        }

        private AS4Message CreateAS4Message()
        {
            UserMessage userMessage = CreateUserMessage();
            this._logger.Info($"[{userMessage.MessageId}] Create AS4Message with Submit Message");

            return this._builder
                .BreakDown()
                .WithSendingPMode(this._internalMessage.SubmitMessage.PMode)
                .WithUserMessage(userMessage)
                .Build();
        }

        private UserMessage CreateUserMessage()
        {
            this._logger.Debug("Map Submit Message to UserMessage");
            
            return Mapper.Map<UserMessage>(this._internalMessage.SubmitMessage);
        }
    }
}
