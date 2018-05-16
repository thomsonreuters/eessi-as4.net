using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Service abstraction to set the referenced deliver message to the right Status/Operation accordingly to the <see cref="DeliverResult"/>.
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
        /// Updates the message Status/Operation accordingly to <see cref="DeliverResult"/>.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public void UpdateDeliverMessageAccordinglyToDeliverResult(string messageId, DeliverResult result)
        {
            Logger.Info($"(Deliver)[{messageId}] Mark deliver message as 'Delivered'");
            Logger.Debug($"(Deliver)[{messageId}] Update InMessage with Status and Operation set to 'Delivered'");


            Logger.Info($"(Deliver)[{messageId}] Update InMessage with Delivered Status and Operation");

            _repository.UpdateInMessage(
                messageId,
                inMessage =>
                {
                    if (result.Status == DeliveryStatus.Successful)
                    {
                        inMessage.SetStatus(InStatus.Delivered);
                        inMessage.SetOperation(Operation.Delivered);
                    }
                    else
                    {
                        (int current, int max) = _repository
                            .GetInMessageData(messageId, m => Tuple.Create(m.CurrentRetryCount, m.MaxRetryCount));

                        if (current < max && result.NeedsAnotherRetry)
                        {
                            inMessage.CurrentRetryCount = current + 1;
                            inMessage.SetOperation(Operation.ToBeDelivered);
                        }
                        else
                        {
                            inMessage.SetStatus(InStatus.Exception);
                            inMessage.SetOperation(Operation.DeadLettered);
                        }
                    }
                });
        }
    }
}
