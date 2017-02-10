using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Describes how the data store gets updated when a message is notified
    /// </summary>
    [Obsolete]
    public class MinderNotifyUpdateInExceptionDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the type <see cref="NotifyUpdateInExceptionDatastoreStep"/> class
        /// </summary>
        public MinderNotifyUpdateInExceptionDatastoreStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the InExceptions table for a given <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            //////NotifyMessage notifyMessage = internalMessage.NotifyMessage;
            //////this._logger.Info($"{internalMessage.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");
            //////using (var context = Registry.Instance.CreateDatastoreContext())
            //////{
            //////    await UpdateInException(internalMessage, new DatastoreRepository(context));
            //////}

            //////return StepResult.Success(internalMessage);
            
            throw new NotImplementedException();
        }

        private static async Task UpdateInException(InternalMessage internalMessage, DatastoreRepository repository)
        {
            SignalMessage signalMessage = internalMessage.AS4Message.PrimarySignalMessage;

            string messageId = signalMessage == null
                ? internalMessage.AS4Message.PrimaryUserMessage.MessageId
                : signalMessage.RefToMessageId;

            await repository.UpdateInExceptionAsync(messageId, UpdateNotifiedInException);
        }

        private static void UpdateNotifiedInException(InException inException)
        {
            inException.Operation = Operation.Notified;
        }
    }
}
