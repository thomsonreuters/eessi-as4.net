using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    public class FilledUserMessage : UserMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilledUserMessage" /> class.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        public FilledUserMessage(string messageId = "message-id")
        {
            Mpc = "mpc";
            MessageId = messageId;
            CollaborationInfo = CreateCollaborationInfo();
            Receiver = CreateParty("Receiver", "org:eu:europa:as4:example");
            Sender = CreateParty("Sender", "org:holodeckb2b:example:company:A");
            MessageProperties = CreateMessageProperties();
        }

        private static CollaborationInfo CreateCollaborationInfo()
        {
            return new CollaborationInfo
            {
                Action = "StoreMessage",
                Service = {
                             Value = "Test", Type = "org:holodeckb2b:services"
                          },
                ConversationId = "org:holodeckb2b:test:conversation",
                AgreementReference = CreateAgreementReference()
            };
        }

        private static AgreementReference CreateAgreementReference()
        {
            return new AgreementReference
            {
                Value = "http://agreements.holodeckb2b.org/examples/agreement0",
                PModeId = "Id"
            };
        }

        private static Party CreateParty(string role, string partyId)
        {
            var partyIds = new List<PartyId> {new PartyId(partyId)};
            return new Party {Role = role, PartyIds = partyIds};
        }

        private static List<MessageProperty> CreateMessageProperties()
        {
            return new List<MessageProperty> {new MessageProperty("Name", "Type", "Value")};
        }
    }
}