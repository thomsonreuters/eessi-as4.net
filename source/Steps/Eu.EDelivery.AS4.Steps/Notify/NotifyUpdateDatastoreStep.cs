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
    public class NotifyUpdateDatastoreStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createDatastoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateDatastoreStep"/> class.
        /// </summary>
        public NotifyUpdateDatastoreStep() : this(Registry.Instance.CreateDatastoreContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateDatastoreStep"/> class.
        /// </summary>
        public NotifyUpdateDatastoreStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
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
            Logger.Info($"{messagingContext.EbmsMessageId} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateDatastoreAsync(notifyMessage).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task UpdateDatastoreAsync(NotifyMessageEnvelope notifyMessage)
        {
            using (var context = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                if (notifyMessage.EntityType == typeof(InMessage))
                {
                    repository.UpdateInMessage(notifyMessage.MessageInfo.MessageId, m =>
                    {
                        m.Status = InStatus.Notified;
                        m.Operation = Operation.Notified;
                    });
                }
                else if (notifyMessage.EntityType == typeof(OutMessage))
                {
                    repository.UpdateOutMessage(notifyMessage.MessageInfo.MessageId, m =>
                    {
                        m.Status = OutStatus.Notified;
                        m.Operation = Operation.Notified;
                    });
                }
                else if (notifyMessage.EntityType == typeof(InException))
                {
                    repository.UpdateInException(notifyMessage.MessageInfo.RefToMessageId, UpdateNotifiedException);
                }
                else if (notifyMessage.EntityType == typeof(OutException))
                {
                    repository.UpdateOutException(notifyMessage.MessageInfo.RefToMessageId, UpdateNotifiedException);
                }
                else
                {
                    throw new InvalidOperationException($"Unable to update notified entities of type {notifyMessage.EntityType.FullName}");
                }

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static void UpdateNotifiedException(ExceptionEntity exceptionMessage)
        {
            exceptionMessage.Operation = Operation.Notified;
        }
    }
}