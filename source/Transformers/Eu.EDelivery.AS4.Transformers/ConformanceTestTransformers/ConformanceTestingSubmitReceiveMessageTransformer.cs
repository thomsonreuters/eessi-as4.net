using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Streaming;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;

namespace Eu.EDelivery.AS4.Transformers.ConformanceTestTransformers
{
    [NotConfigurable]
    [ExcludeFromCodeCoverage]
    public class ConformanceTestingSubmitReceiveMessageTransformer : ITransformer
    {
        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            // We receive an AS4Message from Minder, we should convert it to a SubmitMessage if the action is submit.
            // In any other case, we should just return a MessagingContext which contains the as4Message.
            var receivedStream = VirtualStream.Create();

            await message.UnderlyingStream.CopyToAsync(receivedStream);
            receivedStream.Position = 0;

            var receivedMessage = new ReceivedMessage(receivedStream, message.ContentType);

            try
            {
                var transformer = new AS4MessageTransformer();
                var messagingContext = await transformer.TransformAsync(receivedMessage);

                if (messagingContext.AS4Message == null)
                {
                    throw new InvalidMessageException(
                        "Messaging context must contain an AS4 Message");
                }

                if (messagingContext.AS4Message?.FirstUserMessage?.CollaborationInfo?.Action?.Equals("Submit", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    var as4Message =
                        TransformMinderSubmitToAS4Message(messagingContext.AS4Message.FirstUserMessage, messagingContext.AS4Message.Attachments);
                    messagingContext = new MessagingContext(as4Message, MessagingContextMode.Submit);

                    AssignPModeToContext(messagingContext);

                    return messagingContext;
                }

                receivedStream.Position = 0;
                var receiveContext = new MessagingContext(receivedMessage, MessagingContextMode.Receive);
                receiveContext.ModifyContext(messagingContext.AS4Message);
                return receiveContext;
            }
            catch (Exception ex)
            {
                var l = NLog.LogManager.GetCurrentClassLogger();
                l.Error(ex.Message);
                l.Trace(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    l.Error(ex.InnerException.Message);
                }

                throw;
            }
        }

        private static AS4Message TransformMinderSubmitToAS4Message(UserMessage submitMessage, IEnumerable<Attachment> attachments)
        {
            var userMessage = new UserMessage(
                GetPropertyValue(submitMessage.MessageProperties, "MessageId"),
                GetPropertyValue(submitMessage.MessageProperties, "RefToMessageId"),
                GetCollaborationFromProperties(submitMessage.MessageProperties),
                GetSenderFromSender(submitMessage),
                GetReceiverFromProperties(submitMessage),
                submitMessage.PayloadInfo,
                BlacklistMessageInfoProperties(submitMessage.MessageProperties));

            AS4Message result = AS4Message.Create(userMessage);

            foreach (Attachment attachment in attachments)
            {
                result.AddAttachment(attachment);
            }

            return result;
        }

        private static CollaborationInfo GetCollaborationFromProperties(IEnumerable<MessageProperty> properties)
        {
            // AgreementRef must not be present in the AS4Message for minder.
            return new CollaborationInfo(
                Maybe<AgreementReference>.Nothing,
                new Service(GetPropertyValue(properties, "Service")),
                GetPropertyValue(properties, "Action"),
                GetPropertyValue(properties, "ConversationId"));
        }

        private static Party GetSenderFromSender(UserMessage submitMessage)
        {
            return new Party(
                role: GetPropertyValue(submitMessage.MessageProperties, "FromPartyRole"),
                partyId: new PartyId(
                    id: GetPropertyValue(submitMessage.MessageProperties, "FromPartyId"),
                    type: submitMessage.Sender.PartyIds.First().Type));
        }

        private static Party GetReceiverFromProperties(UserMessage submitMessage)
        {
            return new Party(
                role: GetPropertyValue(submitMessage.MessageProperties, "ToPartyRole"),
                partyId: new PartyId(
                    id: GetPropertyValue(submitMessage.MessageProperties, "ToPartyId"),
                    type: submitMessage.Receiver.PartyIds.First().Type));
        }

        private static IEnumerable<MessageProperty> BlacklistMessageInfoProperties(IEnumerable<MessageProperty> properties)
        {
            string[] whiteList = { "originalSender", "finalRecipient", "trackingIdentifier" };
            return properties.Where(p => whiteList.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
        }

        private static void AssignPModeToContext(MessagingContext context)
        {
            AS4Message as4Message = context.AS4Message;

            // The PMode that must be used is defined in the CollaborationInfo.Service property.
            var pmode = Config.Instance.GetSendingPMode(as4Message.FirstUserMessage.CollaborationInfo.Action);
            context.SendingPMode = pmode;
        }
       
        private static string GetPropertyValue(IEnumerable<MessageProperty> properties, string propertyName)
        {
            return properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}
