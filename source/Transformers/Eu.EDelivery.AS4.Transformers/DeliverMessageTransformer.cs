using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Validators;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    public class DeliverMessageTransformer : ITransformer
    {
        private readonly IValidator<DeliverMessage> _validator = new DeliverMessageValidator();

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="InternalMessage"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var entityMessage = message as ReceivedMessageEntityMessage;

            if (entityMessage == null)
            {
                throw AS4ExceptionBuilder.WithDescription("The message that must be transformed should be of type ReceivedMessageEntityMessage")
                                         .WithErrorCode(ErrorCode.Ebms0009)
                                         .Build();
            }

            // Get the AS4Message that is referred to by this entityMessage.            
            AS4Message as4Message = await RetrieveAS4Message(entityMessage, cancellationToken);

            if (as4Message.UserMessages.Count != 1)
            {
                throw new InvalidOperationException("The AS4Message should contain only one UserMessage.");
            }

            var deliverMessage = CreateDeliverMessage(as4Message.PrimaryUserMessage, as4Message);
            
            ValidateDeliverMessage(deliverMessage);

            var internalMessage = new InternalMessage(as4Message);

            var serialized = AS4XmlSerializer.ToString(deliverMessage);

            internalMessage.DeliverMessage = new DeliverMessageEnvelope(deliverMessage.MessageInfo,
                                                                        Encoding.UTF8.GetBytes(serialized),
                                                                        "application/xml");

            return internalMessage;
        }

        private static async Task<AS4Message> RetrieveAS4Message(ReceivedMessageEntityMessage entityMessage, CancellationToken cancellationToken)
        {
            var as4Transformer = new AS4MessageTransformer();
            var internalMessage = await as4Transformer.TransformAsync(entityMessage, cancellationToken);

            var as4Message = internalMessage.AS4Message;

            // Create the DeliverMessage for this specific UserMessage that has been received.
            var userMessage =
                as4Message.UserMessages.FirstOrDefault(m => m.MessageId.Equals(entityMessage.MessageEntity.EbmsMessageId, StringComparison.OrdinalIgnoreCase));

            if (userMessage == null)
            {
                throw new InvalidOperationException($"The UserMessage with ID {entityMessage.MessageEntity.EbmsMessageId} could not be found in the referenced AS4Message.");
            }

            // Remove all the user- and signalmessages from the AS4Message, except the userMessage that we're about to deliver.
            as4Message.UserMessages.Clear();
            as4Message.UserMessages.Add(userMessage);

            // Remove the attachments that are not part of the UserMessage that is to be delivered.
            List<Attachment> attachments = new List<Attachment>();

            foreach (var partInfo in userMessage.PayloadInfo)
            {
                attachments.Add(as4Message.Attachments.FirstOrDefault(a => a.Matches(partInfo)));
            }

            var attachmentCollection = (List<Attachment>)as4Message.Attachments;

            for (int i = attachmentCollection.Count - 1; i >= 0; i--)
            {

                var attachment = attachmentCollection[i];

                if (attachments.Exists(a => a.Id.Equals(attachment.Id)) == false)
                {
                    attachment.Content.Dispose();
                    attachments.Remove(attachment);
                }
            }

            return as4Message;
        }

        private static DeliverMessage CreateDeliverMessage(UserMessage userMessage, AS4Message as4Message)
        {
            var deliverMessage = AS4Mapper.Map<DeliverMessage>(userMessage);
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
                if (partInfo != null)
                {
                    partInfo.Location = attachment.Location;
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
