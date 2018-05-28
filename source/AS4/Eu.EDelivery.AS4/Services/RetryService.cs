using System;
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
            UpdateDeliverMessage(
                messageId,
                status,
                onSuccess: _ =>
                {
                    Logger.Debug($"(Deliver)[{messageId}] Attachments are uploaded successfully, no retry is needed");
                });
        }

        /// <summary>
        /// Updates the message Status/Operation accordingly to <see cref="Eu.EDelivery.AS4.Strategies.Sender.SendResult"/>.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="status">The deliver status during the delivery of the deliver message.</param>
        /// <returns></returns>
        public void UpdateDeliverMessageForDeliverResult(string messageId, SendResult status)
        {
            UpdateDeliverMessage(
                messageId,
                status,
                onSuccess: inMessage =>
                {
                    Logger.Info($"(Deliver)[{messageId}] Mark deliver message as Delivered");
                    Logger.Debug($"(Deliver)[{messageId}] Update InMessage with Status and Operation set to Delivered");

                    inMessage.SetStatus(InStatus.Delivered);
                    inMessage.SetOperation(Operation.Delivered);
                });
        }

        private void UpdateDeliverMessage(string messageId, SendResult status, Action<InMessage> onSuccess)
        {
            _repository.UpdateInMessage(
                messageId,
                inMessage =>
                {
                    if (status == SendResult.Success)
                    {
                        onSuccess(inMessage);
                    }
                    else
                    {
                        (int current, int max) = _repository
                            .GetInMessageData(messageId, m => Tuple.Create(m.CurrentRetryCount, m.MaxRetryCount));

                        if (current < max && status == SendResult.RetryableFail)
                        {
                            Logger.Info($"(Deliver)[{messageId}] DeliverMessage failed this time, will be retried");
                            Logger.Debug($"(Deliver[{messageId}]) Update InMessage with CurrentRetryCount={current + 1}, Operation=ToBeDelivered");

                            inMessage.CurrentRetryCount = current + 1;
                            inMessage.SetOperation(Operation.ToBeDelivered);
                        }
                        else
                        {
                            Logger.Info($"(Deliver)[{messageId}] DeliverMessage failed during the delivery, exhausted retries");
                            Logger.Debug($"(Deliver)[{messageId}] Update InMessage with Status=Exception, Operation=DeadLettered");

                            inMessage.SetStatus(InStatus.Exception);
                            inMessage.SetOperation(Operation.DeadLettered);
                        }
                    }
                });
        }
    }
}
