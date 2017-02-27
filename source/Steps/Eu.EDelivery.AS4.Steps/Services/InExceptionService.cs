using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Services
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
            this._repository = repository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Insert a given <see cref="AS4Exception"/>
        /// into the Data store
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="as4Message"></param>
        public async Task InsertAS4ExceptionAsync(AS4Exception exception, AS4Message as4Message)
        {
            foreach (string messageId in exception.MessageIds)
            {
                await TryStoreIncomingAS4ExceptionAsync(exception, messageId, as4Message);
            }
        }

        private async Task TryStoreIncomingAS4ExceptionAsync(AS4Exception as4Exception, string messageId, AS4Message as4Message)
        {
            try
            {
                this._logger.Info($"Store InException: {as4Exception.Message}");

                InException inException = CreateInException(as4Exception, messageId);
                SetMessageBody(inException, as4Message);
                await InsertInException(inException);
            }
            catch (Exception exception)
            {
                this._logger.Error("Cannot Update Datastore with InException");
                this._logger.Debug($"Cannot update Datastore: {exception.Message}");
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

        private async Task InsertInException(InException inException)
        {
            await this._repository.InsertInExceptionAsync(inException);
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
        Task InsertAS4ExceptionAsync(AS4Exception exception, AS4Message as4Message);
    }
}
