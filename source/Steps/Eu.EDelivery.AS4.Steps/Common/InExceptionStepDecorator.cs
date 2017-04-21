using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Common
{
    /// <summary>
    /// Exception Handling Step: acts as Decorator for the <see cref="CompositeStep"/>
    /// Responsibility: describes what to do in case an exception occurs within a AS4 Notify operation
    /// </summary>
    public class InExceptionStepDecorator : IStep
    {
        private readonly IStep _step;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="InExceptionStepDecorator"/> class
        /// with a a decorated <paramref name="step"/>
        /// </summary>
        /// <param name="step"></param>
        public InExceptionStepDecorator(IStep step)
        {
            this._step = step;
        }

        /// <summary>
        /// Start executing Step
        /// so it can be catched
        /// </summary>
        /// <param name="internalMessage"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            try
            {
                return await _step.ExecuteAsync(internalMessage, cancellationToken);
            }
            catch (AS4Exception exception)
            {
                using (var context = Registry.Instance.CreateDatastoreContext())
                {
                    var repository = new DatastoreRepository(context);
                    HandleInException(exception, internalMessage, repository);

                    await context.SaveChangesAsync(cancellationToken);
                }

                return StepResult.Failed(exception);
            }
            catch (Exception exception)
            {
                _logger.Fatal($"An unexpected error occured: {exception.Message}");
                return StepResult.Failed(AS4ExceptionBuilder.WithDescription(exception.Message).WithInnerException(exception).Build());
            }
        }

        private void HandleInException(AS4Exception exception, InternalMessage internalMessage, IDatastoreRepository repository)
        {
            _logger.Info($"{internalMessage.Prefix} Handling AS4 Exception...");
            foreach (string messageId in exception.MessageIds)
            {
                TryHandleInException(exception, messageId, internalMessage, repository);
            }
        }

        private void TryHandleInException(AS4Exception exception, string messageId, InternalMessage message, IDatastoreRepository repository)
        {
            try
            {
                InException inException = CreateInException(exception, messageId);

                inException.MessageBody = GetAS4MessageByteRepresentation(message.AS4Message);

                repository.InsertInException(inException);
                UpdateInMessage(messageId, exception.ErrorAlias, repository);
            }
            catch (Exception ex)
            {
                _logger.Error($"{message.Prefix} Cannot Update Datastore with InException: {ex.Message}");

                if (ex.InnerException != null)
                {
                    _logger.Error(ex.InnerException.Message);
                }
            }
        }

        private static InException CreateInException(AS4Exception exception, string messageId)
        {
            return new InExceptionBuilder()
                .WithAS4Exception(exception)
                .WithEbmsMessageId(messageId)
                .Build();
        }

        private static byte[] GetAS4MessageByteRepresentation(AS4Message message)
        {
            using (var messageBodyStream = new MemoryStream())
            {
                var serializer = new SoapEnvelopeSerializer();
                serializer.Serialize(message, messageBodyStream, CancellationToken.None);

                return messageBodyStream.ToArray();
            }
        }

        private static void UpdateInMessage(string messageId, ErrorAlias exceptionType, IDatastoreRepository repository)
        {
            repository.UpdateInMessage(messageId,
                message =>
                {
                    message.Status = InStatus.Exception;
                    message.ErrorAlias = exceptionType;
                });
        }
    }
}