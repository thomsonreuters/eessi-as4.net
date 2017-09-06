using System;
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
    [Obsolete("Use the NotifyUpdateDatastoreStep instead")]
    public class NotifyUpdateInExceptionDatastoreStep : IStep
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the type <see cref="NotifyUpdateInExceptionDatastoreStep"/> class
        /// </summary>
        public NotifyUpdateInExceptionDatastoreStep()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start updating the InExceptions table for a given <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            var notifyMessageEnv = messagingContext.NotifyMessage;
            _logger.Info($"{messagingContext.EbmsMessageId} Update Notify Message {notifyMessageEnv.MessageInfo.MessageId}");

            using (var context = Registry.Instance.CreateDatastoreContext())
            {
                UpdateDatastore(notifyMessageEnv, new DatastoreRepository(context));

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static void UpdateDatastore(NotifyMessageEnvelope notifyMessage, DatastoreRepository repository)
        {
            repository.UpdateInException(notifyMessage.MessageInfo.RefToMessageId, UpdateNotifiedInException);
        }

        private static void UpdateNotifiedInException(InException inException)
        {
            inException.SetOperation(Operation.Notified);
        }
    }
}
