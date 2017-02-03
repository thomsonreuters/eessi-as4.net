using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Common
{
    /// <summary>
    /// Exception Handling Step: acts as Decorator for the <see cref="CompositeStep"/>
    /// Responsibility: describes what to do in case an exception occurs within a AS4 Send/Submit operation
    /// </summary>
    [Info("Out exception decorator")]
    public class OutExceptionStepDecorator : IStep
    {
        private readonly IStep _step;
        private readonly ILogger _logger;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionStepDecorator"/> class
        /// with a given <paramref name="step"/> to decorate and defaults from <see cref="Registry"/>
        /// </summary>
        /// <param name="step"></param>
        public OutExceptionStepDecorator(IStep step)
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

                internalMessage.Exception = exception;

                using (var context = Registry.Instance.CreateDatastoreContext())
                {
                    await HandleOutException(exception, internalMessage, new DatastoreRepository(context));
                }
                return StepResult.Failed(exception, internalMessage);
            }
        }

        private async Task HandleOutException(AS4Exception exception, InternalMessage internalMessage, IDatastoreRepository repository)
        {
            foreach (string messageId in exception.MessageIds)
            {
                await TryHandleOutExceptionAsync(exception, messageId, internalMessage, repository);
            }
        }

        private async Task TryHandleOutExceptionAsync(AS4Exception exception, string messageId, InternalMessage internalMessage, IDatastoreRepository repository)
        {
            try
            {
                OutException outException = CreateOutException(exception, internalMessage, messageId);

#if DEBUG
                outException.MessageBody = GetAS4MessageByteRepresentationSetMessageBody(internalMessage.AS4Message);

#endif
                await repository.InsertOutExceptionAsync(outException);
                await repository.UpdateOutMessageAsync(messageId, UpdateOutMessageType);
            }
            catch (Exception)
            {
                this._logger.Error($"{internalMessage.Prefix} Cannot Update Datastore with OutException");
            }
        }

        private static OutException CreateOutException(AS4Exception exception, InternalMessage internalMessage, string messageId)
        {
            OutExceptionBuilder builder = new OutExceptionBuilder()
                .WithAS4Exception(exception)
                .WithEbmsMessageId(messageId);

            if (NeedsOutExceptionBeNotified(internalMessage.AS4Message?.SendingPMode))
            {
                builder.WithOperation(Operation.ToBeNotified);
            }

            return builder.Build();
        }

        private static byte[] GetAS4MessageByteRepresentationSetMessageBody(AS4Message as4Message)
        {
            using (var messageBodyStream = new MemoryStream())
            {
                var serializer = new SoapEnvelopeSerializer();
                serializer.Serialize(as4Message, messageBodyStream, CancellationToken.None);

                return messageBodyStream.ToArray();
            }
        }

        private static bool NeedsOutExceptionBeNotified(SendingProcessingMode sendPMode)
        {
            return sendPMode?.ExceptionHandling?.NotifyMessageProducer == true;
        }

        private static void UpdateOutMessageType(OutMessage outMessage)
        {
            outMessage.EbmsMessageType = MessageType.Error;
        }
    }
}
