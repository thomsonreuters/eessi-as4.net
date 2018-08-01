using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Service abstraction to set the referenced deliver message to the right Status/Operation accordingly to the <see cref="Eu.EDelivery.AS4.Strategies.Sender.SendResult"/>.
    /// </summary>
    internal class MarkForRetryService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatastoreRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkForRetryService"/> class.
        /// </summary>
        /// <param name="respository">The repository.</param>
        public MarkForRetryService(IDatastoreRepository respository)
        {
            _repository = respository;
        }

        /// <summary>
        /// Updates the message Status/Operation accordingly to the status of a 
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="status">The upload status during the delivery of the payloads.</param>
        public void UpdateDeliverMessageForUploadResult(string messageId, SendResult status)
        {
            _repository.UpdateInMessage(
                messageId: messageId,
                updateAction: entity => UpdateMessageEntity(
                    status: status,
                    entity: entity,
                    getter: s => _repository.GetRetryReliability(r => r.RefToInMessageId == entity.Id, s),
                    onSuccess: _ => Logger.Debug("Attachments are uploaded successfully, no retry is needed"),
                    onFailure: e =>
                    {
                        Logger.Info($"(Deliver)[{entity.EbmsMessageId}] DeliverMessage failed during the delivery, exhausted retries");
                        e.SetStatus(InStatus.Exception);
                    }));
        }

        /// <summary>
        /// Updates the message Status/Operation accordingly to <see cref="Eu.EDelivery.AS4.Strategies.Sender.SendResult"/>.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="status">The deliver status during the delivery of the deliver message.</param>
        /// <returns></returns>
        public void UpdateDeliverMessageForDeliverResult(string messageId, SendResult status)
        {
            _repository.UpdateInMessage(
                messageId: messageId,
                updateAction: entity => UpdateMessageEntity(
                    status: status,
                    entity: entity,
                    getter: s => _repository.GetRetryReliability(r => r.RefToInMessageId == entity.Id, s),
                    onSuccess: e =>
                    {
                        Logger.Info($"(Deliver)[{messageId}] Mark deliver message as Delivered");
                        Logger.Debug("Update InMessage with Status and Operation set to Delivered");

                        e.SetStatus(InStatus.Delivered);
                        e.Operation = Operation.Delivered;
                    },
                    onFailure: e =>
                    {
                        Logger.Info($"(Deliver)[{entity.EbmsMessageId}] DeliverMessage failed during the delivery, exhausted retries");
                        e.SetStatus(InStatus.Exception);
                    }));
        }

        /// <summary>
        /// Updates the NotifyMessage stored as <see cref="InMessage"/> in the datastore, accordingly to the given notification result.
        /// </summary>
        /// <param name="messageId">Identifier to update the <see cref="InMessage"/></param>
        /// <param name="result">Notification result used to determine the right update values for the to be updated entity</param>
        public void UpdateNotifyMessageForIncomingMessage(string messageId, SendResult result)
        {
            _repository.UpdateInMessage(
                messageId: messageId,
                updateAction: entity => UpdateMessageEntity(
                    status: result,
                    entity: entity,
                    getter: selector => _repository.GetRetryReliability(r => r.RefToInMessageId == entity.Id, selector),
                    onSuccess: e =>
                    {
                        Logger.Info($"(Notify)[{messageId}] Mark NotifyMessage as Notified");
                        Logger.Debug("Update InMessage with Status and Operation set to Notified");

                        e.SetStatus(InStatus.Notified);
                        e.Operation = Operation.Notified;
                    },
                    onFailure: m =>
                    {
                        Logger.Info($"(Notify)[{entity.EbmsMessageId}] NotifyMessage failed during the notification, exhausted retries");
                        m.SetStatus(InStatus.Exception);
                    }));
        }

        /// <summary>
        /// Updates the NotifyMessage stored as <see cref="OutMessage"/> in the datastore, accordingly to the given notification result.
        /// </summary>
        /// <param name="messageId">Identifier to update the <see cref="OutMessage"/></param>
        /// <param name="result">Notification result used to determine the right update values for the to be updated entity</param>
        public void UpdateNotifyMessageForOutgoingMessage(long messageId, SendResult result)
        {
            _repository.UpdateOutMessage(
                messageId,
                entity => UpdateMessageEntity(
                    status: result,
                    entity: entity,
                    getter: selector => _repository.GetRetryReliability(r => r.RefToOutMessageId == entity.Id, selector),
                    onSuccess: m =>
                    {
                        Logger.Info($"(Notify)[{messageId}] Mark NotifyMessage as Notified");
                        Logger.Debug($"(Notify)[{messageId}] Update InMessage with Status and Operation set to Notified");

                        m.SetStatus(OutStatus.Notified);
                        m.Operation = Operation.Notified;
                    },
                    onFailure: m =>
                    {
                        Logger.Info($"(Notify)[{entity.EbmsMessageId}] NotifyMessage failed during the notification, exhausted retries");
                        m.SetStatus(OutStatus.Exception);
                    }));
        }

        private static void UpdateMessageEntity<T>(
            SendResult status,
            T entity,
            Func<Expression<Func<RetryReliability, RetryReliability>>, IEnumerable<RetryReliability>> getter,
            Action<T> onSuccess,
            Action<T> onFailure) where T : MessageEntity
        {
            RetryReliability rr = getter(r => r).FirstOrDefault();
            if (status == SendResult.Success)
            {
                onSuccess(entity);

                Logger.Debug("Successful result, so update RetryReliability.Status=Completed");

                if (rr != null)
                {
                    rr.Status = RetryStatus.Completed;
                }
            }
            else
            {
                if (rr == null)
                {
                    Logger.Debug("No retry reliability configured, can't be retried");
                    Logger.Debug($"Update {typeof(T).Name} with {{Status=Exception, Operation=DeadLettered}}");

                    onFailure(entity);
                    entity.Operation = Operation.DeadLettered;
                }
                else if (status == SendResult.RetryableFail)
                {
                    Logger.Info($"[{entity.EbmsMessageId}] Message failed this time, set for retry");
                    Logger.Debug($"Update {typeof(T).Name} with Operation=ToBeRetried");

                    entity.Operation = Operation.ToBeRetried;
                    rr.Status = RetryStatus.Pending;
                }
                else
                {
                    Logger.Info($"[{entity.EbmsMessageId}] Message failed this time due to a fatal result during sending");
                    Logger.Debug($"Update {typeof(T).Name} with Status=Exception, Operation=DeadLettered");

                    onFailure(entity);
                    entity.Operation = Operation.DeadLettered;
                    rr.Status = RetryStatus.Completed;
                }
            }
        }

        /// <summary>
        /// Updates the NotifyMessage stored as <see cref="InException"/> in the datastore, accordingly to the given notification result.
        /// </summary>
        /// <param name="messageId">Identifier to update the <see cref="InException"/></param>
        /// <param name="result">Notification result used to determine the right update values for the to be updated entity</param>
        public void UpdateNotifyExceptionForIncomingMessage(string messageId, SendResult result)
        {
            _repository.UpdateInException(
                refToMessageId: messageId,
                updateAction: exEntity => UpdateExceptionRetry(
                    status: result,
                    entity: exEntity,
                    getter: selector => _repository.GetRetryReliability(r => r.RefToInExceptionId == exEntity.Id, selector)));
        }

        /// <summary>
        /// Updates the NotifyMessage stored as <see cref="OutException"/> in the datastore, accordingly to the given notification result.
        /// </summary>
        /// <param name="messageId">Identifier to update the <see cref="OutException"/></param>
        /// <param name="result">Notification result used to determine the right update values for the to be updated entity</param>
        public void UpdateNotifyExceptionForOutgoingMessage(string messageId, SendResult result)
        {
            _repository.UpdateOutException(
                refToMessageId: messageId,
                updateAction: exEntity => UpdateExceptionRetry(
                    status: result,
                    entity: exEntity,
                    getter: selector => _repository.GetRetryReliability(r => r.RefToOutExceptionId == exEntity.Id, selector)));
        }

        private static void UpdateExceptionRetry<T>(
            SendResult status,
            T entity,
            Func<Expression<Func<RetryReliability, RetryReliability>>, IEnumerable<RetryReliability>> getter) where T : ExceptionEntity
        {
            RetryReliability rr = getter(r => r).FirstOrDefault();
            if (status == SendResult.Success)
            {
                Logger.Info($"(Notify)[{entity.EbmsRefToMessageId}] Mark NotifyMessage as Notified");
                Logger.Debug($"Update {typeof(T).Name} with Operation=Notified");

                entity.Operation = Operation.Notified;
                if (rr != null)
                {
                    rr.Status = RetryStatus.Completed;
                }
            }
            else
            {
                if (rr == null)
                {
                    Logger.Info($"(Notify)[{entity.EbmsRefToMessageId}] Exception NotifyMessage failed during the notification, exhausted retries");
                    Logger.Debug($"Update {typeof(T).Name} with {{Status=Exception, Operation=DeadLettered}}");

                    entity.Operation = Operation.DeadLettered;
                }
                else if (status == SendResult.RetryableFail)
                {
                    Logger.Info($"(Notify)[{entity.EbmsRefToMessageId}] Exception NotifyMessage failed this time, will be retried");
                    Logger.Debug($"Update {typeof(T).Name} with Operation=ToBeRetried");

                    entity.Operation = Operation.ToBeRetried;
                    rr.Status = RetryStatus.Pending;
                }
                else
                {
                    Logger.Info($"(Notify)[{entity.EbmsRefToMessageId}] Exception NotifyMessage failed during the notification, exhausted retries");
                    Logger.Debug($"Update {typeof(T).Name} with {{Status=Exception, Operation=DeadLettered}}");

                    entity.Operation = Operation.DeadLettered;
                    rr.Status = RetryStatus.Completed;
                }
            }
        }
    }
}
