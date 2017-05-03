using System;
using System.IO;
using System.Threading;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Service to expose Data store related operations
    /// for the <see cref="InException"/> Model
    /// </summary>
    public class InExceptionService : IInExceptionService
    {
        private readonly IDatastoreRepository _repository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InExceptionService"/> class. 
        /// Create a Service for the <see cref="InException"/> Model
        /// </summary>
        /// <param name="repository">
        /// </param>
        public InExceptionService(IDatastoreRepository repository)
        {
            _repository = repository;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Insert a given <see cref="AS4Exception"/>
        /// into the Data store
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="as4Message"></param>
        public void InsertAS4Exception(AS4Exception exception, AS4Message as4Message)
        {
            foreach (string messageId in exception.MessageIds)
            {
                TryStoreIncomingAS4Exception(exception, messageId, as4Message);
            }
        }

        private void TryStoreIncomingAS4Exception(AS4Exception as4Exception, string messageId, AS4Message as4Message)
        {
            try
            {
                _logger.Info($"Store InException: {as4Exception.Message}");

                InException inException = CreateInException(as4Exception, messageId);
                SetMessageBody(inException, as4Message);
                InsertInException(inException);
            }
            catch (Exception exception)
            {
                _logger.Error("Cannot Update Datastore with InException");
                _logger.Debug($"Cannot update Datastore: {exception.Message}");
            }
        }

        private static void SetMessageBody(ExceptionEntity outException, AS4Message as4Message)
        {
            using (var messageBodyStream = new MemoryStream())
            {
                var serializer = new SoapEnvelopeSerializer();
                serializer.Serialize(as4Message, messageBodyStream, CancellationToken.None);
                outException.MessageBody = messageBodyStream.ToArray();
            }
        }

        private static InException CreateInException(AS4Exception exception, string messageId)
        {
            return new InExceptionBuilder()
                .WithAS4Exception(exception)
                .WithEbmsMessageId(messageId)
                .Build();
        }

        private void InsertInException(InException inException)
        {
            _repository.InsertInException(inException);
        }
    }

    public interface IInExceptionService
    {
        /// <summary>
        /// Insert a given <see cref="AS4Exception"/>
        /// into the Data store
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="as4Message"></param>
        void InsertAS4Exception(AS4Exception exception, AS4Message as4Message);
    }
}
