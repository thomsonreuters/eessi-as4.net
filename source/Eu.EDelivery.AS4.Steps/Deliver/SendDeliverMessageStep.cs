using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Describes how a DeliverMessage is sent to the consuming business application. 
    /// </summary>
    [Info("Send deliver message to the configured business application endpoint")]
    [Description("Send deliver message to the configured business application endpoint")]
    public class SendDeliverMessageStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDeliverSenderProvider _messageProvider;
        private readonly Func<DatastoreContext> _createDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// </summary>
        public SendDeliverMessageStep()
            : this(
                Registry.Instance.DeliverSenderProvider,
                Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendDeliverMessageStep"/> class
        /// Create a <see cref="IStep"/> implementation
        /// for sending the Deliver Message to the consuming business application
        /// </summary>
        /// <param name="messageProvider"> The message sender provider.</param>
        /// <param name="createDbContext">Creates a new Db Context.</param>
        public SendDeliverMessageStep(
            IDeliverSenderProvider messageProvider,
            Func<DatastoreContext> createDbContext)
        {
            if (messageProvider == null)
            {
                throw new ArgumentNullException(nameof(messageProvider));
            }

            if (createDbContext == null)
            {
                throw new ArgumentNullException(nameof(createDbContext));
            }

            _messageProvider = messageProvider;
            _createDbContext = createDbContext;
        }

        /// <summary>
        /// Start sending the AS4 Messages 
        /// to the consuming business application
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.ReceivingPMode == null)
            {
                throw new InvalidOperationException(
                    "Unable to send DeliverMessage: no ReceivingPMode is set");
            }

            if (messagingContext.ReceivingPMode.MessageHandling?.DeliverInformation == null)
            {
                throw new InvalidOperationException(
                    $"Unable to send the DeliverMessage: the ReceivingPMode {messagingContext.ReceivingPMode.Id} does not contain any <DeliverInformation />." +
                    "Please provide a correct <DeliverInformation /> tag to indicate where the deliver message (and its attachments) should be send to.");
            }

            Logger.Trace($"{messagingContext.LogTag} Start sending the DeliverMessage to the consuming business application...");

            Method deliverMethod = messagingContext.ReceivingPMode.MessageHandling.DeliverInformation.DeliverMethod;

            IDeliverSender sender = _messageProvider.GetDeliverSender(deliverMethod?.Type);
            sender.Configure(deliverMethod);
            SendResult result = await sender.SendAsync(messagingContext.DeliverMessage).ConfigureAwait(false);

            await UpdateDeliverMessage(messagingContext, result);
            return StepResult.Success(messagingContext);
        }

        private async Task UpdateDeliverMessage(MessagingContext messagingContext, SendResult result)
        {
            using (DatastoreContext context = _createDbContext())
            {
                var repository = new DatastoreRepository(context);
                var retryService = new MarkForRetryService(repository);
                retryService.UpdateDeliverMessageForDeliverResult(
                    messagingContext.DeliverMessage.MessageInfo.MessageId, result);

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}