using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Common
{
    /// <summary>
    /// Exception Handling Step: acts as Decorator for the <see cref="CompositeStep"/>
    /// Responsibility: describes what to do in case an exception occurs within a AS4 Send/Submit operation
    /// </summary>
    public class OutExceptionStepDecorator : IStep
    {
        private readonly IStep _step;
        private readonly IDatastoreRepository _repository;
        private readonly ILogger _logger;

        private SendingProcessingMode _sendPMode;
        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionStepDecorator"/> class
        /// with a given <paramref name="step"/> to decorate and defaults from <see cref="Registry"/>
        /// </summary>
        /// <param name="step"></param>
        public OutExceptionStepDecorator(IStep step)
        {
            this._step = step;
            this._repository = Registry.Instance.DatastoreRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionStepDecorator"/> class
        /// Create a Decorator around a given <see cref="IStep"/> implementation
        /// </summary>
        /// <param name="step"> Step to catch </param>
        /// <param name="respository"> used Data store Repository </param>
        public OutExceptionStepDecorator(IStep step, IDatastoreRepository respository)
        {
            this._step = step;
            this._repository = respository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Execute the given Step, so it can be catched
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                return await this._step.ExecuteAsync(internalMessage, cancellationToken);
            }
            catch (AS4Exception exception)
            {
                this._logger.Error(exception.Message);
                InitializeFields(internalMessage, exception);

                await HandleOutException(exception);
                return StepResult.Failed(exception, this._internalMessage);
            }
        }

        private void InitializeFields(InternalMessage internalMessage, AS4Exception exception)
        {
            this._internalMessage = internalMessage;
            this._internalMessage.Exception = exception;
            this._sendPMode = internalMessage.AS4Message?.SendingPMode;
        }

        private async Task HandleOutException(AS4Exception exception)
        {
            foreach (string messageId in exception.MessageIds)
                await TryHandleOutExceptionAsync(exception, messageId);
        }

        private async Task TryHandleOutExceptionAsync(AS4Exception exception, string messageId)
        {
            try
            {
                OutException outException = CreateOutException(exception, messageId);
                await this._repository.InsertOutExceptionAsync(outException);
                await this._repository.UpdateOutMessageAsync(messageId, UpdateOutMessageType);
            }
            catch (Exception)
            {
                this._logger.Error($"{this._internalMessage.Prefix} Cannot Update Datastore with OutException");
            }
        }

        private OutException CreateOutException(AS4Exception exception, string messageId)
        {
            OutExceptionBuilder builder = new OutExceptionBuilder()
                .WithAS4Exception(exception)
                .WithEbmsMessageId(messageId);

            if (NeedsOutExceptionBeNotified())
                builder.WithOperation(Operation.ToBeNotified);

            return builder.Build();
        }

        private bool NeedsOutExceptionBeNotified()
        {
            return this._sendPMode?.ExceptionHandling?.NotifyMessageProducer == true;
        }

        private void UpdateOutMessageType(OutMessage outMessage)
        {
            outMessage.EbmsMessageType = MessageType.Error;
        }
    }
}
