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
    [NotConfigurable]
    public class NotifyUpdateOutMessageDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start updating the Data store for the <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            NotifyMessageEnvelope notifyMessage = messagingContext.NotifyMessage;
            Logger.Info($"{messagingContext.EbmsMessageId} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                repository.UpdateOutMessage(
                    messagingContext.MessageEntityId.Value, 
                    m =>
                    {
                        m.SetStatus(OutStatus.Notified);
                        m.SetOperation(Operation.Notified);
                    });

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return await StepResult.SuccessAsync(messagingContext);

        }
    }
}