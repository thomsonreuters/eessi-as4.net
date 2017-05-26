using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    public class DeliverMessageTransformer : ITransformer
    {
        /// <summary>
        /// Transform a given <see cref="ReceivedMessage" /> to a Canonical <see cref="InternalMessage" /> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var entityMessage = message as ReceivedMessageEntityMessage;

            if (entityMessage == null)
            {
                throw AS4ExceptionBuilder.WithDescription(
                                             "The message that must be transformed should be of type ReceivedMessageEntityMessage")
                                         .WithErrorCode(ErrorCode.Ebms0009)
                                         .Build();
            }

            // Get the AS4Message that is referred to by this entityMessage and modify it so that it just contains
            // the one usermessage that should be delivered.
            InternalMessage transformedMessage = await RetrieveAS4Message(entityMessage, cancellationToken);

            if (transformedMessage.AS4Message.UserMessages.Count != 1)
            {
                throw new InvalidOperationException("The AS4Message should contain only one UserMessage.");
            }

            return transformedMessage;
        }

        /// <summary>
        /// Retrieves the AS4Message that is referenced by the received ReceivedMessage and modify it so that it only contains
        /// a single UserMessage.
        /// </summary>
        /// <param name="entityMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<InternalMessage> RetrieveAS4Message(
            ReceivedMessageEntityMessage entityMessage,
            CancellationToken cancellationToken)
        {
            var as4Transformer = new AS4MessageTransformer();
            InternalMessage internalMessage = await as4Transformer.TransformAsync(entityMessage, cancellationToken);


            AS4Message as4Message = RemoveUnnecessaryMessages(
                internalMessage.AS4Message,
                entityMessage.MessageEntity.EbmsMessageId);

            as4Message = RemoveUnnecessaryAttachments(as4Message);

            return new InternalMessage(as4Message)
            {
                ReceivingPMode = internalMessage.ReceivingPMode,
                SendingPMode = internalMessage.SendingPMode
            };
        }

        private static AS4Message RemoveUnnecessaryMessages(AS4Message as4Message, string userMessageId)
        {
            // Create the DeliverMessage for this specific UserMessage that has been received.
            UserMessage userMessage =
                as4Message.UserMessages.FirstOrDefault(
                    m => m.MessageId.Equals(userMessageId, StringComparison.OrdinalIgnoreCase));

            if (userMessage == null)
            {
                throw new InvalidOperationException(
                    $"The UserMessage with ID {userMessageId} could not be found in the referenced AS4Message.");
            }

            // Remove all the user- and signalmessages from the AS4Message, except the userMessage that we're about to deliver.
            as4Message.SignalMessages.Clear();
            as4Message.UserMessages.Clear();
            as4Message.UserMessages.Add(userMessage);

            return as4Message;
        }

        private static AS4Message RemoveUnnecessaryAttachments(AS4Message as4Message)
        {
            // Remove the attachments that are not part of the UserMessage that is to be delivered.
            List<Attachment> attachments = SelectToBeDeliveredUserMessageAttachments(as4Message);

            var attachmentCollection = (List<Attachment>)as4Message.Attachments;

            for (int i = attachmentCollection.Count - 1; i >= 0; i--)
            {
                Attachment attachment = attachmentCollection[i];

                if (attachments.Exists(a => a.Id.Equals(attachment.Id)) == false)
                {
                    attachment.Content.Dispose();
                    attachments.Remove(attachment);
                }
            }

            return as4Message;
        }

        private static List<Attachment> SelectToBeDeliveredUserMessageAttachments(AS4Message as4Message)
        {
            return
                as4Message.PrimaryUserMessage.PayloadInfo.Select(
                    partInfo => as4Message.Attachments.FirstOrDefault(a => a.Matches(partInfo))).ToList();
        }
    }
}