using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Describes how the data store gets updated when a message is notified
    /// </summary>
    public class NotifyUpdateInExceptionDatastoreStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IDatastoreRepository _respository;

        /// <summary>
        /// Initializes a new instance of the type <see cref="NotifyUpdateInExceptionDatastoreStep"/> class
        /// </summary>
        public NotifyUpdateInExceptionDatastoreStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
            this._respository = Registry.Instance.DatastoreRepository;
        }

        /// <summary>
        /// Initializes a new instance of the type <see cref="NotifyUpdateInExceptionDatastoreStep"/> class
        /// </summary>
        public NotifyUpdateInExceptionDatastoreStep(IDatastoreRepository repository)
        {
            this._respository = repository;
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the InExceptions table for a given <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            NotifyMessage notifyMessage = internalMessage.NotifyMessage;
            this._logger.Info($"{internalMessage.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateDatastoreAsync(notifyMessage);
            return StepResult.Success(internalMessage);
        }

        private async Task UpdateDatastoreAsync(NotifyMessage notifyMessage)
        {
            await this._respository.UpdateInExceptionAsync(
                notifyMessage.MessageInfo.RefToMessageId, UpdateNotifiedInException);
        }

        private void UpdateNotifiedInException(InException inException)
        {
            inException.Operation = Operation.Notified;
        }
    }
}
