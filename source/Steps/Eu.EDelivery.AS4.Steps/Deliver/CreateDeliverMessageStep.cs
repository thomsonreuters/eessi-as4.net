using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
    /// Describes how an <see cref="AS4Message"/> 
    /// is being used to create a <see cref="DeliverMessage"/>
    /// </summary>
    public class CreateDeliverMessageStep : IStep
    {
        private readonly ILogger _logger;
        private readonly IValidator<DeliverMessage> _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDeliverMessageStep"/> class. 
        /// Create a <see cref="IStep"/> implementation
        /// to create <see cref="DeliverMessage"/> Models
        /// </summary>
        public CreateDeliverMessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
            this._validator = new DeliverMessageValidator();
        }

        /// <summary>
        /// Starting creation of the <see cref="DeliverMessage"/>
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info($"{internalMessage.Prefix} Create a Deliver Message from an AS4 Message");


            var deliverMessage = CreateDeliverMessage(internalMessage.AS4Message);


            var serialized = AS4XmlSerializer.Serialize(deliverMessage);

            ValidateDeliverMessage(deliverMessage, internalMessage);

            internalMessage.DeliverMessage = new DeliverMessageEnvelope(deliverMessage.MessageInfo,
                                                                        System.Text.Encoding.UTF8.GetBytes(serialized),
                                                                        "application/xml");

            return await StepResult.SuccessAsync(internalMessage);
        }

        private static DeliverMessage CreateDeliverMessage(AS4Message as4Message)
        {
            var deliverMessage = AS4Mapper.Map<DeliverMessage>(as4Message.PrimaryUserMessage);
            AssignSendingPModeId(as4Message, deliverMessage);
            AssignAttachmentLocations(as4Message, deliverMessage);

            return deliverMessage;
        }

        private static void AssignSendingPModeId(AS4Message as4Message, DeliverMessage deliverMessage)
        {
            deliverMessage.CollaborationInfo.AgreementRef.PModeId = as4Message.SendingPMode.Id ?? string.Empty;
        }

        private static void AssignAttachmentLocations(AS4Message as4Message, DeliverMessage deliverMessage)
        {
            foreach (Attachment attachment in as4Message.Attachments)
            {
                Payload partInfo = deliverMessage.Payloads.FirstOrDefault(p => p.Id.Contains(attachment.Id));
                if (partInfo != null) partInfo.Location = Path.GetFullPath(attachment.Location);
            }
        }

        private void ValidateDeliverMessage(DeliverMessage deliverMessage, InternalMessage internalMessage)
        {
            if (!this._validator.Validate(deliverMessage))
            {
                _logger.Error($"{internalMessage.Prefix} DeliverMessage is not valid.");
                return;
            }

            string messageId = deliverMessage.MessageInfo.MessageId;
            string message = $"{internalMessage.Prefix} Deliver Message {messageId} was valid";
            this._logger.Debug(message);
        }
    }
}