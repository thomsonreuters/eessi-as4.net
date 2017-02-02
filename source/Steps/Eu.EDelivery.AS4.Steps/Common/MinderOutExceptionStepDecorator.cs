using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Common
{
    /// <summary>
    /// Exception Handling Decorator for Minder Conformance Testing,
    /// by creating an AS4 Message Error Message as User Message
    /// </summary>
    [Obsolete("We can use the regular OutExceptionStepDecorator for Minder tests. Only difference is MessageBody is set.")]
    public class MinderOutExceptionStepDecorator : IStep
    {
        private readonly IStep _step;
        private readonly ILogger _logger;

        private SendingProcessingMode _sendPMode;
        private InternalMessage _internalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionStepDecorator"/> class
        /// with a given <paramref name="step"/> to decorate and defaults from <see cref="Registry"/>
        /// </summary>
        /// <param name="step"></param>
        public MinderOutExceptionStepDecorator(IStep step)
        {
            this._step = step;
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

                using (var context = Registry.Instance.CreateDatastoreContext())
                {
                    await HandleOutException(exception, new DatastoreRepository(context));
                    this._internalMessage.Exception = exception;
                }

                return StepResult.Failed(exception, this._internalMessage);
            }
        }

        private void InitializeFields(InternalMessage internalMessage, AS4Exception exception)
        {
            this._internalMessage = internalMessage;
            this._internalMessage.Exception = exception;
            this._sendPMode = internalMessage.AS4Message?.SendingPMode;
        }

        private async Task HandleOutException(AS4Exception exception, IDatastoreRepository repository)
        {
            foreach (string messageId in exception.MessageIds)
                await TryHandleOutExceptionAsync(exception, messageId, repository);
        }

        private async Task TryHandleOutExceptionAsync(AS4Exception exception, string messageId, IDatastoreRepository repository)
        {
            try
            {
                OutException outException = CreateOutException(exception, messageId);
                SetMessageBody(outException);

                await repository.InsertOutExceptionAsync(outException);
                await repository.UpdateOutMessageAsync(messageId, UpdateOutMessageType);
            }
            catch (Exception)
            {
                this._logger.Error($"{this._internalMessage.Prefix} Cannot Update Datastore with OutException");
            }
        }

        private void SetMessageBody(ExceptionEntity outException)
        {
            using (var messageBodyStream = new MemoryStream())
            {
                var serializer = new SoapEnvelopeSerializer();
                serializer.Serialize(this._internalMessage.AS4Message, messageBodyStream, CancellationToken.None);
                outException.MessageBody = messageBodyStream.ToArray();
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

        private static void UpdateOutMessageType(OutMessage outMessage)
        {
            outMessage.EbmsMessageType = MessageType.Error;
        }
    }
}
