using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Describes how the data store gets updated when a message is notified
    /// </summary>
    public class NotifyUpdateOutMessageDatastoreStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IDatastoreRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateOutMessageDatastoreStep"/> class. 
        /// Create a <see cref="IStep"/> implementation to update the data store
        /// according to the given <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        public NotifyUpdateOutMessageDatastoreStep(IDatastoreRepository repository)
        {
            this._repository = repository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public NotifyUpdateOutMessageDatastoreStep()
        {
            this._repository = Registry.Instance.DatastoreRepository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store for the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception when data store is unavailable</exception>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            NotifyMessage notifyMessage = internalMessage.NotifyMessage;
            this._logger.Info($"{internalMessage.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateDatastoreAync(notifyMessage);
            return StepResult.Success(internalMessage);
        }

        private async Task UpdateDatastoreAync(NotifyMessage notifyMessage)
        {
            await this._repository.UpdateOutMessage(
                notifyMessage.MessageInfo.MessageId, UpdateNotifiedOutMessage);
        }

        private void UpdateNotifiedOutMessage(OutMessage outMessage)
        {
            outMessage.Status = OutStatus.Notified;
            outMessage.Operation = Operation.Notified;
        }
    }
}