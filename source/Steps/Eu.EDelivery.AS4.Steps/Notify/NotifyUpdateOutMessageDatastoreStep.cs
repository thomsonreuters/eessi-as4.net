using System;
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

        public NotifyUpdateOutMessageDatastoreStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
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
            var notifyMessage = internalMessage.NotifyMessage;
            _logger.Info($"{internalMessage.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateDatastoreAsync(notifyMessage).ConfigureAwait(false);
            return await StepResult.SuccessAsync(internalMessage);
            
        }

        private static async Task UpdateDatastoreAsync(NotifyMessageEnvelope notifyMessage)
        {
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                repository.UpdateOutMessage(notifyMessage.MessageInfo.MessageId, UpdateNotifiedOutMessage);

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static void UpdateNotifiedOutMessage(OutMessage outMessage)
        {
            outMessage.Status = OutStatus.Notified;
            outMessage.Operation = Operation.Notified;
        }
    }
}