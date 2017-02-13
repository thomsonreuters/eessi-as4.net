using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Submit;

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

            if (as4Message?.PrimaryUserMessage?.CollaborationInfo?.Action?.Equals("Submit",
                    StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var submitMessage = CreateSubmitMessageFromAS4Message(as4Message);
                return new InternalMessage(submitMessage);
            }
            else
            {
                return new InternalMessage(as4Message);
            }
        }

        private SubmitMessage CreateSubmitMessageFromAS4Message(AS4Message as4message)
        {
            var submitMessage = new SubmitMessage();

            AssignMessageProperties(submitMessage, as4message);

            return submitMessage;
        }

        private void AssignMessageProperties(SubmitMessage submitMessage, AS4Message as4Message)
        {
            AssignMessageInfoProperties(submitMessage, as4Message);
            AssignConversationIdProperty(submitMessage, as4Message);
            AssignSenderProperties(submitMessage, as4Message);
            AssignReceiverProperties(submitMessage, as4Message);
            AssignServiceActionProperties(submitMessage, as4Message);
        }

        private string GetAS4MessageProperty(AS4Message as4Message, string propertyName)
        {
            return as4Message.PrimaryUserMessage.MessageProperties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        private void AssignMessageInfoProperties(SubmitMessage submitMessage, AS4Message as4Message)
        {
            submitMessage.MessageInfo.MessageId = GetAS4MessageProperty(as4Message, "MessageId");
            submitMessage.MessageInfo.RefToMessageId = GetAS4MessageProperty(as4Message, "RefToMessageId");

            // userMessage.Timestamp = DateTimeOffset.UtcNow;
        }

        private void AssignConversationIdProperty(SubmitMessage submitMessage, AS4Message as4Message)
        {
            submitMessage.Collaboration.ConversationId = GetAS4MessageProperty(as4Message, "ConverstationId");
        }

        private void AssignSenderProperties(SubmitMessage userMessage, AS4Message as4Message)
        {
            var fromPartyId = new Model.Common.PartyId()
            {
                Id = GetAS4MessageProperty(as4Message, "FromPartyId"),
                Type = ""
            };

            userMessage.PartyInfo.FromParty = new Model.Common.Party()
            {
                PartyIds = new[] { fromPartyId },
                Role = GetAS4MessageProperty(as4Message, "FromPartyRole")
            };

        }

        private void AssignReceiverProperties(SubmitMessage userMessage, AS4Message as4message)
        {
            var toPartyId = new Model.Common.PartyId()
            {
                Id = GetAS4MessageProperty(as4message, "ToPartyId"),
                Type = ""
            };

            userMessage.PartyInfo.ToParty = new Model.Common.Party()
            {
                PartyIds = new[] { toPartyId },
                Role = GetAS4MessageProperty(as4message, "ToPartyRole")
            };
        }

        private void AssignServiceActionProperties(SubmitMessage userMessage, AS4Message as4message)
        {
            userMessage.Collaboration.Service.Value = GetAS4MessageProperty(as4message, "Service");
            userMessage.Collaboration.Action = GetAS4MessageProperty(as4message, "Action");
            userMessage.Collaboration.AgreementRef.PModeId = GetAS4MessageProperty(as4message, "Action");
        }
    }
}
