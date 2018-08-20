using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers.InteropTestTransformers
{
    [ExcludeFromCodeCoverage]
    public class InteropTestingSubmitReceiveMessageTransformer : ITransformer
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
            // In any other case, we should just return an MessagingContext which contains the as4Message.
            var transformer = new AS4MessageTransformer();
            var messagingContext = await transformer.TransformAsync(message);

            var as4Message = messagingContext.AS4Message;
            
            if (as4Message?.FirstUserMessage?.CollaborationInfo?.Action?.Equals("Submit", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var properties = as4Message.FirstUserMessage?.MessageProperties;

                UserMessage transformed = TransformUserMessage(as4Message.FirstUserMessage, properties);
                as4Message.UpdateMessageUnit(as4Message.FirstUserMessage, transformed);

                messagingContext = new MessagingContext(as4Message, MessagingContextMode.Submit);

                AssignPModeToContext(messagingContext);

                return messagingContext;               
            }

            return new MessagingContext(as4Message, MessagingContextMode.Receive);
        }

        private static void AssignPModeToContext(MessagingContext message)
        {
            AS4Message as4Message = message.AS4Message;

            // The PMode that should be used can be determind by concatenating several items to create the PMode ID
            // - CollaborationInfo.Action
            // - ToParty
            string pmodeKey = $"{as4Message.FirstUserMessage.CollaborationInfo.Action}_FROM_{as4Message.FirstUserMessage.Sender.PartyIds.First().Id}_TO_{as4Message.FirstUserMessage.Receiver.PartyIds.First().Id}";

            // The PMode that must be used is defined in the CollaborationInfo.Service property.
            var pmode = Config.Instance.GetSendingPMode(pmodeKey);

            message.SendingPMode = pmode;
        }

        private static UserMessage TransformUserMessage(UserMessage userMessage, IEnumerable<MessageProperty> properties)
        {
            return new UserMessage(
                GetPropertyValue(properties, "MessageId"),
                GetPropertyValue(properties, "RefToMessageId"),
                GetCollaborationFromProperties(properties),
                GetSenderFromproperties(properties),
                GetReceiverFromProperties(properties),
                new PartInfo[0],
                WhiteListedMessageProperties(userMessage));
        }

        private static IEnumerable<MessageProperty> WhiteListedMessageProperties(UserMessage userMessage)
        {
            string[] whiteList = { "originalSender", "finalRecipient", "trackingIdentifier", "TA_Id" };
            return userMessage.MessageProperties
                       .Where(p => whiteList.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
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

        private static Party GetReceiverFromProperties(IEnumerable<MessageProperty> properties)
        {
            return new Party(
                role: GetPropertyValue(properties, "ToPartyRole"),
                partyId: new PartyId(
                    id: GetPropertyValue(properties, "ToPartyId"),
                    type: GetPropertyValue(properties, "ToPartyType")));
        }

        private static Party GetSenderFromproperties(IEnumerable<MessageProperty> properties)
        {
            return new Party(
                role: GetPropertyValue(properties, "FromPartyRole"),
                partyId: new PartyId(
                    id: GetPropertyValue(properties, "FromPartyId"),
                    type: GetPropertyValue(properties, "FromPartyType")));
        }

        private static string GetPropertyValue(IEnumerable<MessageProperty> properties, string propertyName)
        {
            return properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}