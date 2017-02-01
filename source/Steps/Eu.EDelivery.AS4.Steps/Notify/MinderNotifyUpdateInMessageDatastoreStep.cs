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
    public class MinderNotifyUpdateInMessageDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateInExceptionDatastoreStep"/> class
        /// </summary>
        public MinderNotifyUpdateInMessageDatastoreStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store for the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            NotifyMessage notifyMessage = internalMessage.NotifyMessage;
            this._logger.Info($"{internalMessage.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateInMessage(internalMessage);

            return StepResult.Success(internalMessage);
        }

        private static async Task UpdateInMessage(InternalMessage internalMessage)
        {
            SignalMessage signalMessage = internalMessage.AS4Message.PrimarySignalMessage;

            string messageId = signalMessage == null
                ? internalMessage.AS4Message.PrimaryUserMessage.MessageId
                : signalMessage.MessageId;
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                await repository.UpdateInMessageAsync(messageId, UpdateNotifiedInMessage);
            }
        }

        private static void UpdateNotifiedInMessage(InMessage inMessage)
        {
            inMessage.Status = InStatus.Notified;
            inMessage.Operation = Operation.Notified;
        }
    }
}