using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Transformers
{
    [ExcludeFromCodeCoverage]
    public class InteropTestingSubmitReceiveMessageTransformer : ITransformer
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

                // This is an ugly hack, but we need something to use in our ConditionalStep in order to know whether or not we should submit or receive.
                // This needs to be reviewed later.  It is very possible that we can get rid of the SubmitMessage - property in the InternalMessage, which
                // will be an opportunity to refactor this.
                var result = new InternalMessage(as4Message);
                result.SubmitMessage.Collaboration.Action = "Submit";

                return result;
            }

            return new InternalMessage(as4Message);
        }

        private static void AssignPMode(AS4Message as4Message)
        {
            // The PMode that should be used can be determind by concatenating several items to create the PMode ID
            // - CollaborationInfo.Action
            // - ToParty

            string pModeKey = $"{as4Message.PrimaryUserMessage.CollaborationInfo.Action}_FROM_{as4Message.PrimaryUserMessage.Sender.PartyIds.First().Id}_TO_{as4Message.PrimaryUserMessage.Receiver.PartyIds.First().Id}";

            // The PMode that must be used is defined in the CollaborationInfo.Service property.
            var pmode = Config.Instance.GetSendingPMode(pModeKey);

            as4Message.SendingPMode = pmode;
        }

        private static void TransformUserMessage(UserMessage userMessage, IList<MessageProperty> properties)
        {
            SetMessageInfoProperties(userMessage, properties);
            SetCollaborationInfoProperties(userMessage, properties);
            SetPartyProperties(userMessage, properties);

            RemoveMessageInfoProperties(userMessage);
        }

        private static void RemoveMessageInfoProperties(UserMessage userMessage)
        {
            string[] whiteList = { "originalSender", "finalRecipient", "trackingIdentifier", "TA_Id" };

            userMessage.MessageProperties = userMessage.MessageProperties.Where(p => whiteList.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private static void SetMessageInfoProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            userMessage.MessageId = GetPropertyValue(properties, "MessageId");
            userMessage.RefToMessageId = GetPropertyValue(properties, "RefToMessageId");
            userMessage.Timestamp = DateTimeOffset.UtcNow;
        }

        private static void SetCollaborationInfoProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            userMessage.CollaborationInfo.ConversationId = GetPropertyValue(properties, "ConversationId");
            userMessage.CollaborationInfo.Service.Value = GetPropertyValue(properties, "Service");
            userMessage.CollaborationInfo.Action = GetPropertyValue(properties, "Action");

            // AgreementRef must not be present in the AS4Message for testing.
            userMessage.CollaborationInfo.AgreementReference = null;
        }

        private static void SetPartyProperties(UserMessage userMessage, IList<MessageProperty> properties)
        {
            userMessage.Sender.PartyIds.First().Id = GetPropertyValue(properties, "FromPartyId");
            userMessage.Sender.PartyIds.First().Type = GetPropertyValue(properties, "FromPartyType");
            userMessage.Sender.Role = GetPropertyValue(properties, "FromPartyRole");

            userMessage.Receiver.PartyIds.First().Id = GetPropertyValue(properties, "ToPartyId");
            userMessage.Receiver.PartyIds.First().Type = GetPropertyValue(properties, "ToPartyType");
            userMessage.Receiver.Role = GetPropertyValue(properties, "ToPartyRole");
        }

        private static string GetPropertyValue(IList<MessageProperty> properties, string propertyName)
        {
            return properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.Value;
        }
    }
}