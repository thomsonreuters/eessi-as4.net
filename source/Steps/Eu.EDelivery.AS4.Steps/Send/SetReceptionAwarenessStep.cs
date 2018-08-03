using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how the state and configuration on the retry mechanism of reception awareness is stored
    /// </summary>
    [Info("Set reception awareness for the message")]
    [Description(
        "This step makes sure that reception awareness is enabled for the message that is to be sent, " +
        "if reception awareness is enabled in the sending PMode.")]
    public class SetReceptionAwarenessStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createDatastore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetReceptionAwarenessStep"/> class.
        /// </summary>
        public SetReceptionAwarenessStep() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetReceptionAwarenessStep"/> class.
        /// </summary>
        /// <param name="createDatastore"></param>
        public SetReceptionAwarenessStep(Func<DatastoreContext> createDatastore)
        {
            _createDatastore = createDatastore;
        }

        /// <summary>
        /// Start configuring Reception Awareness
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (!IsReceptionAwarenessEnabled(messagingContext))
            {
                string pmodeId = messagingContext.SendingPMode.Id;
                return await ReturnSameResult(messagingContext, $"Reception Awareness is not enabled in SendingPMode {pmodeId}");
            }

            await InsertReceptionAwarenessAsync(messagingContext).ConfigureAwait(false);
            return StepResult.Success(messagingContext);
        }

        private static bool IsReceptionAwarenessEnabled(MessagingContext messagingContext)
        {
            return messagingContext.SendingPMode.Reliability?.ReceptionAwareness?.IsEnabled == true;
        }

        private static async Task<StepResult> ReturnSameResult(MessagingContext messagingContext, string description)
        {
            Logger.Info($"{messagingContext.LogTag} {description}");
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task InsertReceptionAwarenessAsync(MessagingContext messagingContext)
        {
            Logger.Info($"{messagingContext.LogTag} Set Reception Awareness");

            using (DatastoreContext context = _createDatastore())
            {
                var repository = new DatastoreRepository(context);

                // Create the ReceptionAwareness record for the OutMessage
                InsertReceptionAwarenessForMessages(messagingContext, repository);

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static void InsertReceptionAwarenessForMessages(
            MessagingContext context,
            IDatastoreRepository repository)
        {
            if (context.MessageEntityId == null)
            {
                throw new InvalidOperationException(
                    $"{context.LogTag} Unable to retrieve the OutMessage information from the MessagingContext.ReceivedMessage");
            }

            Entities.ReceptionAwareness receptionAwareness =
                CreateReceptionAwareness(
                    context.MessageEntityId.Value,
                    context.AS4Message.GetPrimaryMessageId(),
                    context.SendingPMode);

            repository.InsertReceptionAwareness(receptionAwareness);
        }

        private static Entities.ReceptionAwareness CreateReceptionAwareness(
            long outMessageId,
            string ebmsMessageId,
            SendingProcessingMode pmode)
        {
            // The Message hasn't been sent yet, so set the currentretrycount to -1 and the lastsendtime to null.
            // The SendMessageStep will update those values once the context has in fact been sent.
            var receptionAwareness = new Entities.ReceptionAwareness(outMessageId, ebmsMessageId)
            {
                CurrentRetryCount = -1,
                TotalRetryCount = pmode.Reliability.ReceptionAwareness.RetryCount,
                RetryInterval = pmode.Reliability.ReceptionAwareness.RetryInterval,
                LastSendTime = null,
            };

            receptionAwareness.Status = ReceptionStatus.Pending;

            return receptionAwareness;
        }
    }
}