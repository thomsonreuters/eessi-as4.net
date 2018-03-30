using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    [Description("This step makes sure that the status of the message is set to �Notified� after notification")]
    [Info("Update datastore after notification")]
    public class NotifyUpdateDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createDatastoreContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateDatastoreStep" /> class.
        /// </summary>
        public NotifyUpdateDatastoreStep() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUpdateDatastoreStep" /> class.
        /// </summary>
        public NotifyUpdateDatastoreStep(Func<DatastoreContext> createDatastoreContext)
        {
            _createDatastoreContext = createDatastoreContext;
        }

        /// <summary>
        /// Start updating the Data store for the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            var notifyMessage = messagingContext.NotifyMessage;
            Logger.Info($"{messagingContext.EbmsMessageId} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            await UpdateDatastoreAsync(notifyMessage, messagingContext).ConfigureAwait(false);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task UpdateDatastoreAsync(NotifyMessageEnvelope notifyMessage, MessagingContext messagingContext)
        {
            using (DatastoreContext context = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                if (notifyMessage.EntityType == typeof(InMessage))
                {
                    repository.UpdateInMessage(notifyMessage.MessageInfo.MessageId, m =>
                    {
                        m.SetStatus(InStatus.Notified);
                        m.SetOperation(Operation.Notified);
                    });
                }
                else if (notifyMessage.EntityType == typeof(OutMessage) && messagingContext.MessageEntityId != null)
                {
                    repository.UpdateOutMessage(messagingContext.MessageEntityId.Value, m =>
                    {
                        m.SetStatus(OutStatus.Notified);
                        m.SetOperation(Operation.Notified);
                    });
                }
                else if (notifyMessage.EntityType == typeof(InException))
                {
                    repository.UpdateInException(notifyMessage.MessageInfo.RefToMessageId, ex => ex.SetOperation(Operation.Notified));
                }
                else if (notifyMessage.EntityType == typeof(OutException))
                {
                    repository.UpdateOutException(notifyMessage.MessageInfo.RefToMessageId, ex => ex.SetOperation(Operation.Notified));
                }
                else
                {
                    throw new InvalidOperationException($"Unable to update notified entities of type {notifyMessage.EntityType.FullName}");
                }

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}