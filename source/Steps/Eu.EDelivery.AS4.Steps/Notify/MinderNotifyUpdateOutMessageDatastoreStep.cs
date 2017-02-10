using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
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
    public class MinderNotifyUpdateOutMessageDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateOutMessageDatastoreStep"/> class. 
        /// Create a <see cref="IStep"/> implementation to update the data store
        /// according to the given <see cref="NotifyMessage"/>
        /// </summary>                              
        public MinderNotifyUpdateOutMessageDatastoreStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store for the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AS4Exception">Throws exception when data store is unavailable</exception>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            //NotifyMessage notifyMessage = internalMessage.NotifyMessage;
            //this._logger.Info($"{internalMessage.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            //await UpdateOutMessage(internalMessage);
            //return StepResult.Success(internalMessage);
            throw new NotImplementedException();
        }

        private static async Task UpdateOutMessage(InternalMessage internalMessage)
        {
            SignalMessage signalMessage = internalMessage.AS4Message.PrimarySignalMessage;

            string messageId = signalMessage == null
                ? internalMessage.AS4Message.PrimaryUserMessage.MessageId
                : signalMessage.MessageId;
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                await repository.UpdateOutMessageAsync(messageId, UpdateNotifiedOutMessage);
            }
        }

        private static void UpdateNotifiedOutMessage(OutMessage outMessage)
        {
            outMessage.Status = OutStatus.Notified;
            outMessage.Operation = Operation.Notified;
        }
    }
}