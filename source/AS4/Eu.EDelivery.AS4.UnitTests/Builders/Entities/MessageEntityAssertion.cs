using System;
using System.Linq;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Entities
{
    public class MessageEntityAssertion
    {
        /// <summary>
        /// Asserts the party information.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        public static void AssertPartyInfo(AS4Message expected, MessageEntity actual)
        {
            Func<Party, string> getPartyId = p => p.PartyIds.First().Id;
            Assert.Equal(getPartyId(expected.PrimaryUserMessage.Sender), actual.FromParty);
            Assert.Equal(getPartyId(expected.PrimaryUserMessage.Receiver), actual.ToParty);
        }

        /// <summary>
        /// Asserts the collaboration information.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        public static void AssertCollaborationInfo(AS4Message expected, MessageEntity actual)
        {
            CollaborationInfo expectedCollaboration = expected.PrimaryUserMessage.CollaborationInfo;
            Assert.Equal(expectedCollaboration.Action, actual.Action);
            Assert.Equal(expectedCollaboration.ConversationId, actual.ConversationId);
            Assert.Equal(expectedCollaboration.Service.Value, actual.Service);
        }

        /// <summary>
        /// Asserts the meta information.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        public static void AssertUserMessageMetaInfo(AS4Message expected, MessageEntity actual)
        {
            Assert.Equal(expected.PrimaryUserMessage.IsTest, actual.IsTest);
            Assert.Equal(expected.PrimaryUserMessage.IsDuplicate, actual.IsDuplicate);
        }

        /// <summary>
        /// Asserts the signal message meta information.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        public static void AssertSignalMessageMetaInfo(AS4Message expected, MessageEntity actual)
        {
            Assert.Equal(expected.PrimarySignalMessage.IsDuplicate, actual.IsDuplicate);
        }

        /// <summary>
        /// Asserts the SOAP envelope.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="actual">The actual.</param>
        public static void AssertSoapEnvelope(MessageUnit expected, MessageEntity actual)
        {
            string xmlRepresentation = AS4XmlSerializer.ToString(AS4Mapper.Map<Xml.UserMessage>(expected));

            Assert.Equal(xmlRepresentation, actual.SoapEnvelope);
        }
    }
}
