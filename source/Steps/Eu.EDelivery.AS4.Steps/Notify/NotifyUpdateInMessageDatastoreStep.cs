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
    public class NotifyUpdateInMessageDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateInExceptionDatastoreStep"/> class
        /// </summary>
        public NotifyUpdateInMessageDatastoreStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the Data store for the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            var notifyMessage = messagingContext.NotifyMessage;
            _logger.Info($"{messagingContext.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateDatastoreAync(notifyMessage).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private static async Task UpdateDatastoreAync(NotifyMessageEnvelope notifyMessage)
        {
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                repository.UpdateInMessage(notifyMessage.MessageInfo.MessageId, UpdateNotifiedInMessage);

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static void UpdateNotifiedInMessage(InMessage inMessage)
        {
            inMessage.Status = InStatus.Notified;
            inMessage.Operation = Operation.Notified;
        }
    }
}