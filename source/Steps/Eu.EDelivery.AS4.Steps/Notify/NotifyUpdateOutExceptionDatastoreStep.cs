﻿using System;
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
    public class NotifyUpdateOutExceptionDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start updating the OutExceptions table for a given <see cref="NotifyMessage"/>
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            NotifyMessageEnvelope notifyMessage = messagingContext.NotifyMessage;
            Logger.Info($"{messagingContext.EbmsMessageId} Update Notify Message {notifyMessage.MessageInfo.MessageId}");

            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                repository.UpdateOutException(
                    notifyMessage.MessageInfo.RefToMessageId,
                    ex => ex.SetOperation(Operation.Notified));

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            return StepResult.Success(messagingContext);
        }
    }
}
