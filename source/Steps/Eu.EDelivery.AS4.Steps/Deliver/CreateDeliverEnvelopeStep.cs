using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// <see cref="IStep" /> implementation to create a <see cref="DeliverMessage" />.
    /// </summary>
    public class CreateDeliverEnvelopeStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IValidator<DeliverMessage> _validator = new DeliverMessageValidator();

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            messagingContext.DeliverMessage = await CreateDeliverMessageEnvelope(messagingContext);

            return StepResult.Success(messagingContext);
        }

        private async Task<DeliverMessageEnvelope> CreateDeliverMessageEnvelope(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;
            DeliverMessage deliverMessage = CreateDeliverMessage(as4Message.PrimaryUserMessage, messagingContext);

            ValidateDeliverMessage(deliverMessage);

            string serialized = await AS4XmlSerializer.ToStringAsync(deliverMessage);

            return new DeliverMessageEnvelope(
                deliverMessage.MessageInfo,
                Encoding.UTF8.GetBytes(serialized),
                "application/xml");
        }

        private static DeliverMessage CreateDeliverMessage(UserMessage userMessage, MessagingContext message)
        {
            var deliverMessage = AS4Mapper.Map<DeliverMessage>(userMessage);
            AssignSendingPModeId(message, deliverMessage);
            AssignAttachmentLocations(message.AS4Message, deliverMessage);

            return deliverMessage;
        }

        private static void AssignSendingPModeId(MessagingContext message, DeliverMessage deliverMessage)
        {
            deliverMessage.CollaborationInfo.AgreementRef.PModeId = message.SendingPMode?.Id ?? string.Empty;
        }

        private static void AssignAttachmentLocations(AS4Message as4Message, DeliverMessage deliverMessage)
        {
            foreach (Attachment attachment in as4Message.Attachments)
            {
                Payload partInfo = deliverMessage.Payloads.FirstOrDefault(p => p.Id.Contains(attachment.Id));

                if (partInfo != null)
                {
                    partInfo.Location = attachment.Location ?? string.Empty;
                }
            }
        }

        private void ValidateDeliverMessage(DeliverMessage deliverMessage)
        {
            _validator.Validate(deliverMessage);

            string messageId = deliverMessage.MessageInfo.MessageId;
            string message = $"Deliver Message {messageId} was valid";

            Logger.Debug(message);
        }
    }
}