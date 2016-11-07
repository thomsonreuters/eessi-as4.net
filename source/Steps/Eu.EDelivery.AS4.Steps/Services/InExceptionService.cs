using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Repositories;
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
        public async Task InsertAS4ExceptionAsync(AS4Exception exception)
        {
            foreach (string messageId in exception.MessageIds)
                await TryInserIncomingAS4ExceptionAsync(exception, messageId);
        }

        private async Task TryInserIncomingAS4ExceptionAsync(AS4Exception as4Exception, string messageId)
        {
            try
            {
                this._logger.Info($"Store InException: {as4Exception.Message}");
                InException inException = CreateInException(as4Exception, messageId);
                await this._repository.InsertInExceptionAsync(inException);
            }
            catch (Exception exception)
            {
                this._logger.Error("Cannot Update Datastore with InException");
                this._logger.Debug($"Cannot update Datastore: {exception.Message}");
            }
        }

        private InException CreateInException(AS4Exception exception, string messageId)
        {
            return new InExceptionBuilder()
                .WithAS4Exception(exception)
                .WithEbmsMessageId(messageId)
                .Build();
        }
    }

    public interface IInExceptionService
    {
        /// <summary>
        /// Insert a given <see cref="AS4Exception"/>
        /// into the Data store
        /// </summary>
        /// <param name="exception"></param>
        Task InsertAS4ExceptionAsync(AS4Exception exception);
    }
}
