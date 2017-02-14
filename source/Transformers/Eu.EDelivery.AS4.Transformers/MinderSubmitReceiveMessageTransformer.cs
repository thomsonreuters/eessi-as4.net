using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{

    public class MinderSubmitReceiveMessageTransformer : ITransformer
    {
        public async Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            // We receive an AS4Message from Minder, we should convert it to a SubmitMessage if the action is submit.
            // In any other case, we should just return an InternalMessage which contains the as4Message.
            var transformer = new AS4MessageTransformer();
            var internalMessage = await transformer.TransformAsync(message, cancellationToken);

            var as4Message = internalMessage.AS4Message;

            if (as4Message?.PrimaryUserMessage?.CollaborationInfo?.Action?.Equals("Submit", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var properties = as4Message.PrimaryUserMessage?.MessageProperties;

                TransformUserMessage(as4Message.PrimaryUserMessage, properties);

                AssignPMode(as4Message);
            }

            return new InternalMessage(as4Message);
        }

        private void AssignPMode(AS4Message as4Message)
        {
            // The PMode that must be used is defined in the CollaborationInfo.Service property.
            var pmode = Config.Instance.GetSendingPMode(as4Message.PrimaryUserMessage.CollaborationInfo.Action);
            as4Message.SendingPMode = pmode;
        }

        private void TransformUserMessage(UserMessage userMessage, IList<MessageProperty> properties)
        {
            SetMessageInfoProperties(userMessage, properties);
            SetCollaborationInfoProperties(userMessage, properties);
            SetPartyProperties(userMessage, properties);

            RemoveMessageInfoProperties(userMessage);

            userMessage.MessageProperties.Add(new MessageProperty("Operation", "Submit"));
        }

        private void RemoveMessageInfoProperties(UserMessage userMessage)
        {
            string[] whiteList = { "originalSender", "finalRecipient", "trackingIdentifier" };

            userMessage.MessageProperties = userMessage.MessageProperties.Where(p => whiteList.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private void SetMessageInfoProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            userMessage.MessageId = GetPropertyValue(properties, "MessageId");
            userMessage.RefToMessageId = GetPropertyValue(properties, "RefToMessageId");
            userMessage.Timestamp = DateTimeOffset.UtcNow;
        }

        private void SetCollaborationInfoProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            userMessage.CollaborationInfo.ConversationId = GetPropertyValue(properties, "ConversationId");
            userMessage.CollaborationInfo.Service.Value = GetPropertyValue(properties, "Service");
            userMessage.CollaborationInfo.Action = GetPropertyValue(properties, "Action");

            // AgreementRef must not be present in the AS4Message for minder.
            userMessage.CollaborationInfo.AgreementReference = null;
        }

        private void SetPartyProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            userMessage.Sender.PartyIds.First().Id = GetPropertyValue(properties, "FromPartyId");
            userMessage.Sender.Role = GetPropertyValue(properties, "FromPartyRole");

            userMessage.Receiver.PartyIds.First().Id = GetPropertyValue(properties, "ToPartyId");
            userMessage.Receiver.Role = GetPropertyValue(properties, "ToPartyRole");
        }

        private static string GetPropertyValue(IList<MessageProperty> properties, string propertyName)
        {
            return properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}
