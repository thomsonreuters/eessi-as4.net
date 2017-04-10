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
        private readonly ILogger _logger;
        private readonly AS4MessageBuilder _builder;
        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAS4MessageStep"/> class
        /// </summary>
        public CreateAS4MessageStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _builder = new AS4MessageBuilder();
        }

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
                _internalMessage.AS4Message = CreateAS4Message();

                return await StepResult.SuccessAsync(internalMessage);
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
            _logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithSendingPMode(_internalMessage.AS4Message.SendingPMode)
                .WithMessageIds(generatedMessageId)
                .WithInnerException(innerException)
                .Build();
        }

        private AS4Message CreateAS4Message()
        {
            UserMessage userMessage = CreateUserMessage();
            _logger.Info($"[{userMessage.MessageId}] Create AS4Message with Submit Message");

            return _builder
                .BreakDown()
                .WithSendingPMode(_internalMessage.SubmitMessage.PMode)
                .WithUserMessage(userMessage)
                .Build();
        }

        private UserMessage CreateUserMessage()
        {
            _logger.Debug("Map Submit Message to UserMessage");
            
            return AS4Mapper.Map<UserMessage>(_internalMessage.SubmitMessage);
        }
    }
}
