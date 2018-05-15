using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
    [Description("Step that creates a deliver message")]
    [Info("Create deliver message")]
    public class CreateDeliverEnvelopeStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly DeliverMessageValidator _validator = new DeliverMessageValidator();

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            AS4Message as4Message = messagingContext.AS4Message;
            DeliverMessage deliverMessage = CreateDeliverMessage(as4Message.PrimaryUserMessage, messagingContext);
            ValidateDeliverMessage(deliverMessage);

            string serialized = await AS4XmlSerializer.ToStringAsync(deliverMessage);
            var envelope = new DeliverMessageEnvelope(
                messageInfo: deliverMessage.MessageInfo,
                deliverMessage: Encoding.UTF8.GetBytes(serialized),
                contentType: "application/xml",
                payloads: as4Message.Attachments);

            messagingContext.ModifyContext(envelope);
            return StepResult.Success(messagingContext);
        }

        private static DeliverMessage CreateDeliverMessage(UserMessage userMessage, MessagingContext context)
        {
            var deliverMessage = AS4Mapper.Map<DeliverMessage>(userMessage);
            deliverMessage.CollaborationInfo.AgreementRef.PModeId = context.SendingPMode?.Id ?? string.Empty;

            foreach (Attachment attachment in context.AS4Message.Attachments)
            {
                Payload partInfo = deliverMessage.Payloads.FirstOrDefault(p => p.Id.Contains(attachment.Id));

                if (partInfo != null)
                {
                    partInfo.Location = attachment.Location ?? string.Empty;
                }
            }

            return deliverMessage;
        }

        private void ValidateDeliverMessage(DeliverMessage deliverMessage)
        {
            _validator.Validate(deliverMessage).Result(
                onValidationSuccess: result =>
                {
                    string messageId = deliverMessage.MessageInfo.MessageId;
                    string message = $"Deliver Message {messageId} was valid";

                    Logger.Debug(message);
                },
                onValidationFailed: result =>
                {
                    string description = $"Deliver Message {deliverMessage.MessageInfo.MessageId} was invalid:";
                    string errorMessage = result.AppendValidationErrorsToErrorMessage(description);

                    throw new InvalidDataException(errorMessage);
                });
        }
    }
}