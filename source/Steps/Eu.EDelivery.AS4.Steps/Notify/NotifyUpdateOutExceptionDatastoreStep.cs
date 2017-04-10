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
    public class NotifyUpdateOutExceptionDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the type <see cref="NotifyUpdateInExceptionDatastoreStep"/> class
        /// </summary>
        public NotifyUpdateOutExceptionDatastoreStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the OutExceptions table for a given <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            var notifyMessage = internalMessage.NotifyMessage;
            _logger.Info($"{internalMessage.Prefix} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateDatastoreAsync(notifyMessage);
            return await StepResult.SuccessAsync(internalMessage);
        }

        private static async Task UpdateDatastoreAsync(NotifyMessageEnvelope notifyMessage)
        {
            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                repository.UpdateOutException(notifyMessage.MessageInfo.RefToMessageId, UpdateNotifiedOutException);

                await context.SaveChangesAsync();
            }
        }

        private static void UpdateNotifiedOutException(OutException outException)
        {
            outException.Operation = Operation.Notified;
        }
    }
}
