using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    public class FilledUserMessage : UserMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilledUserMessage" /> class.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="attachmentId">The attachment identifier.</param>
        public FilledUserMessage(string messageId = "message-id", string attachmentId = "attachment-uri") : this(
            messageId,
            attachmentIds: attachmentId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilledUserMessage"/> class.
        /// </summary>
        public FilledUserMessage(string messageId, params string[] attachmentIds)
            : base(
                messageId,
                "mpc",
                CreateCollaborationInfo(),
                CreateParty("Sender", "org:holodeckb2b:example:company:A"),
                CreateParty("Receiver", "org:eu:europa:as4:example"),
                CreatePartInfos(attachmentIds),
                CreateMessageProperties()) { }

        private static IEnumerable<PartInfo> CreatePartInfos(string[] attachmentsIds)
        {
            return attachmentsIds
                   .DefaultIfEmpty("attachment-uri")
                   .Select(id => new PartInfo(href: $"cid:{id}"));
        }

        private static CollaborationInfo CreateCollaborationInfo()
        {
            return new CollaborationInfo(
                agreement: CreateAgreementReference(),
                service: new Service(value: "Test", type: "org:holodeckb2b:services"),
                action: "StoreMessage",
                conversationId: "org:holodeckb2b:test:conversation");
        }

        private static AgreementReference CreateAgreementReference()
        {
            return new AgreementReference(
                value: "http://agreements.holodeckb2b.org/examples/agreement0",
                type: "agreement",
                pModeId: "Id");
        }

        private static Party CreateParty(string role, string partyId)
        {
            return new Party(role, new PartyId(partyId, type: $"https://eu.edelivery.as4.{role}"));
        }

        private static List<MessageProperty> CreateMessageProperties()
        {
            return new List<MessageProperty> { new MessageProperty("Name", "Value", "Type") };
        }
    }
}