using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    public class DeliverMessageTransformer : ITransformer
    {
        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage" /> to a Canonical <see cref="MessagingContext" /> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            var entityMessage = message as ReceivedEntityMessage;
            if (entityMessage == null || !(entityMessage.Entity is MessageEntity me))
            {
                throw new InvalidDataException(
                    $"The message that must be transformed should be of type {nameof(ReceivedEntityMessage)} with a {nameof(MessageEntity)} as Entity");
            }

            // Get the AS4Message that is referred to by this entityMessage and modify it so that it just contains
            // the one usermessage that should be delivered.
            MessagingContext transformedMessage = await RetrieveAS4Message(me.EbmsMessageId, entityMessage);

            if (transformedMessage.AS4Message.UserMessages.Any() == false)
            {
                throw new InvalidOperationException(
                    $"Incoming AS4Message stream from {message.Origin} should contain only a single UserMessage");
            }

            return transformedMessage;
        }

        private static async Task<MessagingContext> RetrieveAS4Message(
            string ebmsMessageId,
            ReceivedMessage entityMessage)
        {
            var as4Transformer = new AS4MessageTransformer();
            MessagingContext messagingContext = await as4Transformer.TransformAsync(entityMessage);

            AS4Message as4Message = RemoveUnnecessaryMessages(
                messagingContext.AS4Message,
                ebmsMessageId);

            as4Message = RemoveUnnecessaryAttachments(as4Message);

            messagingContext.ModifyContext(as4Message);

            return messagingContext;
        }

        private static AS4Message RemoveUnnecessaryMessages(AS4Message as4Message, string userMessageId)
        {
            // Create the DeliverMessage for this specific UserMessage that has been received.
            UserMessage userMessage =
                as4Message.UserMessages.FirstOrDefault(
                    m => m.MessageId.Equals(userMessageId, StringComparison.OrdinalIgnoreCase));

            if (userMessage == null)
            {
                throw new DataException(
                    $"The UserMessage with ID {userMessageId} could not be found in the referenced AS4Message.");
            }

            // Remove all the user- and signalmessages from the AS4Message, except the userMessage that we're about to deliver.
            as4Message.ClearMessageUnits();
            as4Message.AddMessageUnit(userMessage);

            return as4Message;
        }

        private static AS4Message RemoveUnnecessaryAttachments(AS4Message as4Message)
        {
            // Remove the attachments that are not part of the UserMessage that is to be delivered.
            List<Attachment> attachments = SelectToBeDeliveredUserMessageAttachments(as4Message);

            foreach (Attachment attachment in as4Message.Attachments)
            {
                if (attachments.Contains(attachment) == false)
                {
                    as4Message.RemoveAttachment(attachment);
                }
            }

            return as4Message;
        }

        private static List<Attachment> SelectToBeDeliveredUserMessageAttachments(AS4Message as4Message)
        {
            return as4Message.FirstUserMessage.PayloadInfo
                .Select(partInfo => as4Message.Attachments.FirstOrDefault(a => a.Matches(partInfo)))
                .Where(a => a != null)
                .ToList();
        }
    }
}