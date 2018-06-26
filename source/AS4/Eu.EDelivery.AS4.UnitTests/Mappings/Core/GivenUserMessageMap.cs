using System;
using System.Linq;
using Eu.EDelivery.AS4.Functional;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    public class GivenUserMessageMap
    {
        [Fact]
        public void SucceedsFromXmlToCore_IfActionIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.CollaborationInfo = new CollaborationInfo {Action = "StoreMessage"};

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage.CollaborationInfo.Action, actualMessage.CollaborationInfo.Action);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfAgreementIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.CollaborationInfo = new CollaborationInfo
            {
                AgreementRef = new AgreementRef {Value = "http://agreements.holodeckb2b.org/examples/agreement1"}
            };

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage.CollaborationInfo.AgreementRef.Value, actualMessage.CollaborationInfo.AgreementReference.Value);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfConversationIdIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.CollaborationInfo = new CollaborationInfo {ConversationId = "org:holodeckb2b:example:conversation"};

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage.CollaborationInfo.ConversationId, actualMessage.CollaborationInfo.ConversationId);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfFromPartyIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.PartyInfo = new PartyInfo
            {
                From = new From {Role = "Sender", PartyId = new[] {new PartyId {Value = "org:holodeckb2b:example:company:D"}}}
            };

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage.PartyInfo.From.Role, actualMessage.Sender.Role);
            Assert.Equal(expectedMessage.PartyInfo.From.PartyId.First().Value, actualMessage.Sender.PartyIds.First().Id);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfMessageIdIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.MessageInfo.MessageId = "56a5b21c-2b46-45a8-9c7c-0114f109a336@CLT-SMOREELS.ad.codit.eu";

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage.MessageInfo.MessageId, actualMessage.MessageId);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfPayloadIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.PayloadInfo = new[] {new PartInfo {href = "cid:56a5b21c-2b46-45a8-9c7c-0114f109a33-1376138493@CLT-SMOREELS.ad.codit.eu"}};

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage.PayloadInfo.First().href, actualMessage.PayloadInfo.First().Href);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfServiceIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.CollaborationInfo = new CollaborationInfo {Service = new Service {type = "org:holodeckb2b:services", Value = "Examples"}};

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Service expectedService = expectedMessage.CollaborationInfo.Service;
            AS4.Model.Core.Service actualService = actualMessage.CollaborationInfo.Service;

            Assert.Equal(Maybe.Just(expectedService.type), actualService.Type);
            Assert.Equal(expectedService.Value, actualService.Value);
        }

        [Fact]
        public void SucceedsFromXmlToCore_IfToPartyIsFilled()
        {
            // Arrange
            UserMessage expectedMessage = CreateAnonymousMessage();
            expectedMessage.PartyInfo = new PartyInfo
            {
                To = new To {Role = "Receiver", PartyId = new[] {new PartyId {Value = "org:holodeckb2b:example:company:C"}}}
            };

            // Act
            var actualMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage.PartyInfo.To.Role, actualMessage.Receiver.Role);
            Assert.Equal(expectedMessage.PartyInfo.To.PartyId.First().Value, actualMessage.Receiver.PartyIds.First().Id);
        }

        private static UserMessage CreateAnonymousMessage()
        {
            return new UserMessage { MessageInfo = new MessageInfo { Timestamp = DateTime.UtcNow } };
        }
    }
}