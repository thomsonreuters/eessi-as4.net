using System;
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
    public class RetryService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatastoreRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryService"/> class.
        /// </summary>
        /// <param name="respository">The repository.</param>
        public RetryService(IDatastoreRepository respository)
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
                    getter: s => _repository.GetRetryReliability(r => r.RefToInMessageId == entity.Id, s).First(),
                    onSuccess: _ => Logger.Debug($"(Deliver)[{messageId}] Attachments are uploaded successfully, no retry is needed"),
                    onFailure: e => e.SetStatus(InStatus.Exception)));
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
                    getter: s => _repository.GetRetryReliability(r => r.RefToInMessageId == entity.Id, s).First(),
                    onSuccess: e =>
                    {
                        Logger.Info($"(Deliver)[{messageId}] Mark deliver message as Delivered");
                        Logger.Debug($"(Deliver)[{messageId}] Update InMessage with Status and Operation set to Delivered");

                        e.SetStatus(InStatus.Delivered);
                        e.SetOperation(Operation.Delivered);
                    },
                    onFailure: e => e.SetStatus(InStatus.Exception)));
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
                    getter: selector => _repository.GetRetryReliability(r => r.RefToInMessageId == entity.Id, selector).First(),
                    onSuccess: e =>
                    {
                        Logger.Info($"(Notify)[{messageId}] Mark NotifyMessage as Notified");
                        Logger.Debug($"(Notify)[{messageId}] Update InMessage with Status and Operation set to Notified");

                        e.SetStatus(InStatus.Notified);
                        e.SetOperation(Operation.Notified);
                    },
                    onFailure: m => m.SetStatus(InStatus.Exception)));
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
                    getter: selector => _repository.GetRetryReliability(r => r.RefToOutMessageId == entity.Id, selector).First(),
                    onSuccess: m =>
                    {
                        Logger.Info($"(Notify)[{messageId}] Mark NotifyMessage as Notified");
                        Logger.Debug($"(Notify)[{messageId}] Update InMessage with Status and Operation set to Notified");

                        m.SetStatus(OutStatus.Notified);
                        m.SetOperation(Operation.Notified);
                    },
                    onFailure: m => m.SetStatus(OutStatus.Exception)));
        }

        private void UpdateMessageEntity<T>(
            SendResult status,
            T entity,
            Func<Expression<Func<RetryReliability, RetryReliability>>, RetryReliability> getter,
            Action<T> onSuccess,
            Action<T> onFailure) where T : MessageEntity
        {
            if (status == SendResult.Success)
            {
                onSuccess(entity);
            }
            else
            {
                (string type, string action) =
                    entity.Operation == Operation.Delivering.ToString()
                        ? ("Deliver", "delivery")
                        : ("Notify", "notification");

                RetryReliability rr = getter(r => r);
                if (rr.CurrentRetryCount < rr.MaxRetryCount && status == SendResult.RetryableFail)
                {
                    
                    Logger.Info($"({type})[{entity.EbmsMessageId}] {type}Message failed this time, will be retried");
                    Logger.Debug($"({type})[{entity.EbmsMessageId}]) Update {typeof(T).Name} with CurrentRetryCount={rr.CurrentRetryCount + 1}, Operation=ToBeRetried");

                    rr.CurrentRetryCount = rr.CurrentRetryCount + 1;
                    entity.SetOperation(Operation.ToBeRetried);
                }
                else
                {
                    Logger.Info($"({type})[{entity.EbmsMessageId}] {type}Message failed during the {action}, exhausted retries");
                    Logger.Debug($"({type})[{entity.EbmsMessageId}] Update {typeof(T).Name} with Status=Exception, Operation=DeadLettered");

                    onFailure(entity);
                    entity.SetOperation(Operation.DeadLettered);
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
                    getter: selector => _repository.GetRetryReliability(r => r.RefToInExceptionId == exEntity.Id, selector).First()));
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
                    getter: selector => _repository.GetRetryReliability(r => r.RefToOutExceptionId == exEntity.Id, selector).First()));
        }

        private void UpdateExceptionRetry<T>(
            SendResult status,
            T entity,
            Func<Expression<Func<RetryReliability, RetryReliability>>, RetryReliability> getter) where T : ExceptionEntity
        {
            if (status == SendResult.Success)
            {
                Logger.Info($"(Notify)[{entity.EbmsRefToMessageId}] Mark NotifyMessage as Notified");
                Logger.Debug($"(Notify)[{entity.EbmsRefToMessageId}] Update {typeof(T).Name} with Status and Operation set to Notified");

                entity.SetOperation(Operation.Notified);
            }
            else
            {
                RetryReliability rr = getter(r => r);
                if (rr.CurrentRetryCount < rr.MaxRetryCount && status == SendResult.RetryableFail)
                {

                    Logger.Info($"(Notify)[{entity.EbmsRefToMessageId}] Exception NotifyMessage failed this time, will be retried");
                    Logger.Debug($"(Notify)[{entity.EbmsRefToMessageId}]) Update {typeof(T).Name} with CurrentRetryCount={rr.CurrentRetryCount + 1}, Operation=ToBeRetried");

                    rr.CurrentRetryCount = rr.CurrentRetryCount + 1;
                    entity.SetOperation(Operation.ToBeRetried);
                }
                else
                {
                    Logger.Info($"(Notify)[{entity.EbmsRefToMessageId}] Exception NotifyMessage failed during the notification, exhausted retries");
                    Logger.Debug($"(Notify)[{entity.EbmsRefToMessageId}] Update {typeof(T).Name} with Status=Exception, Operation=DeadLettered");

                    entity.SetOperation(Operation.DeadLettered);
                }
            }
        }
    }
}
