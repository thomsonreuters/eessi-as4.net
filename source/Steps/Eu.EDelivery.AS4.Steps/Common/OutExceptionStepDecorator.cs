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
            _step = step;

            _logger = LogManager.GetCurrentClassLogger();
        }


        /// <summary>
        /// Execute the given Step, so it can be catched
        /// </summary>
        /// <param name="messagingContext"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            try
            {
                return await _step.ExecuteAsync(messagingContext, cancellationToken).ConfigureAwait(false);
            }
            catch (AS4Exception exception)
            {
                _logger.Error(exception.Message);

                messagingContext.Exception = exception;
                try
                {
                    using (var context = Registry.Instance.CreateDatastoreContext())
                    {
                        HandleOutException(exception, messagingContext, new DatastoreRepository(context));

                        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Fatal($"An unexpected error occured: {ex.Message}");

                }
                return StepResult.Failed(exception, messagingContext);
            }
        }

        private void HandleOutException(AS4Exception exception, MessagingContext messagingContext, IDatastoreRepository repository)
        {
            foreach (string messageId in exception.MessageIds)
            {
                TryHandleOutException(exception, messageId, messagingContext, repository);
            }
        }

        private void TryHandleOutException(AS4Exception exception, string messageId, MessagingContext messagingContext, IDatastoreRepository repository)
        {
            try
            {
                OutException outException = CreateOutException(exception, messagingContext, messageId);

                // We only need this in some cases ...
                // TODO: only set this if we have no EbmsMessageId ?
                outException.MessageBody = GetAS4MessageByteRepresentationSetMessageBody(messagingContext.AS4Message);

                repository.InsertOutException(outException);
                repository.UpdateOutMessage(messageId, UpdateOutMessageType);
            }
            catch (Exception ex)
            {
                _logger.Error($"{messagingContext.Prefix} Cannot Update Datastore with OutException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.Error(ex.InnerException.Message);
                }
            }
        }

        private static OutException CreateOutException(AS4Exception exception, MessagingContext messagingContext, string messageId)
        {
            OutExceptionBuilder builder = OutExceptionBuilder.ForAS4Exception(exception);
            builder.WithEbmsMessageId(messageId);

            if (NeedsOutExceptionBeNotified(messagingContext?.SendingPMode))
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
