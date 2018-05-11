using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how the data store gets updated when an incoming message is delivered
    /// </summary>
    [Info("Update message status after delivery")]
    [Description("This step makes sure that the status of the message is correctly set after the message has been delivered.")]
    public class DeliverUpdateDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Start updating the InMessages
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);

                Logger.Info($"{messagingContext.Logging} Mark deliver message as 'Delivered'");
                Logger.Debug($"{messagingContext.Logging} Update InMessage with Status and Operation set to 'Delivered'");

                repository.UpdateInMessage(
                    messagingContext.DeliverMessage.MessageInfo.MessageId, 
                    inMessage =>
                    {
                        inMessage.SetStatus(InStatus.Delivered);
                        inMessage.SetOperation(Operation.Delivered);
                    });

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}
