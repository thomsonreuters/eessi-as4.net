using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="DeliverMessageValidator" />
    /// </summary>
    public class GivenDeliverMessageValidatorFacts
    {
        [Fact]
        public void ThenDeliverMessageIsValid()
        {
            TestDeliverMessageValidation(message => { }, expectedValid: true);
        }

        [Theory]
        [InlineData("message-id", null)]
        [InlineData(null, "mpc")]
        public void ThenDeliverMessageIsInvalidWithMissingMessageInfo(string messageId, string mpc)
        {
            TestDeliverMessageValidation(
                message =>
                {
                    message.MessageInfo.MessageId = messageId;
                    message.MessageInfo.Mpc = mpc;
                }, 
                expectedValid: false);
        }

        [Theory]
        [InlineData("type", null)]
        public void ThenDeliverMessageIsInvalidWithMissingService(string type, string value)
        {
            TestDeliverMessageValidation(
                message =>
                {
                    message.CollaborationInfo.Service.Type = type;
                    message.CollaborationInfo.Service.Value = value;
                }, 
                expectedValid: false);
        }

        [Theory]
        [InlineData(null)]
        public void ThenDeliverMessageIsInvalidWithMissingPModeId(string pmodeId)
        {
            TestDeliverMessageValidation(
                message => message.CollaborationInfo.AgreementRef.PModeId = pmodeId, expectedValid: false);
        }

        [Theory]
        [InlineData(null)]
        public void ThenDeliverMessageIsInvalidWithMissingAction(string action)
        {
            TestDeliverMessageValidation(
                message => message.CollaborationInfo.Action = action, expectedValid: false);
        }

        [Theory]
        [InlineData(null)]
        public void ThenDeliverMessageIsInvalidWithMissingConversationId(string conversationId)
        {
            TestDeliverMessageValidation(
                message => message.CollaborationInfo.ConversationId = conversationId, expectedValid: false);
        }

        [Theory]
        [InlineData("Sender", "Id", null, true)]
        [InlineData("Sender", null, "Type", false)]
        public void ThenDeliverMessageIsInvalidWithMissingFromParty(string partyRole, string partyId, string partyType, bool expected)
        {
            TestDeliverMessageValidation(
                message =>
                {
                    message.PartyInfo.FromParty.Role = partyRole;
                    message.PartyInfo.FromParty.PartyIds.First().Id = partyId;
                    message.PartyInfo.FromParty.PartyIds.First().Type = partyType;
                }, 
                expectedValid: expected);
        }

        [Theory]
        [InlineData("Receiver", "Id", null, true)]
        [InlineData("Receiver", null, "Type", false)]
        public void ThenDeliverMessageIsInvalidWithMissingToParty(string partyRole, string partyId, string partyType, bool expected)
        {
            TestDeliverMessageValidation(
                message =>
                {
                    message.PartyInfo.ToParty.Role = partyRole;
                    message.PartyInfo.ToParty.PartyIds.First().Id = partyId;
                    message.PartyInfo.ToParty.PartyIds.First().Type = partyType;
                }, 
                expectedValid: expected);
        }

        private static void TestDeliverMessageValidation(Action<DeliverMessage> arrangeMessage, bool expectedValid)
        {
            // Arrange
            DeliverMessage message = CreateValidDeliverMessage();
            arrangeMessage(message);

            var sut = new DeliverMessageValidator();

            // Act
            ValidationResult result = sut.Validate(message);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        private static DeliverMessage CreateValidDeliverMessage()
        {
            return new DeliverMessage
            {
                MessageInfo = CreateValidMessageInfo(),
                CollaborationInfo = CreateValidCollaborationInfo(),
                PartyInfo = CreateValidPartyInfo()
            };
        }

        private static MessageInfo CreateValidMessageInfo()
        {
            return new MessageInfo {MessageId = Guid.NewGuid().ToString(), Mpc = Guid.NewGuid().ToString()};
        }

        private static CollaborationInfo CreateValidCollaborationInfo()
        {
            return new CollaborationInfo
            {
                Action = Constants.Namespaces.TestAction,
                ConversationId = "1",
                Service = {Type = "StoreServices", Value = Constants.Namespaces.TestService},
                AgreementRef = {PModeId = "pmode-id"}
            };
        }

        private static PartyInfo CreateValidPartyInfo()
        {
            return new PartyInfo {FromParty = CreateValidParty("Sender"), ToParty = CreateValidParty("Receiver")};
        }

        private static Party CreateValidParty(string role)
        {
            return new Party {Role = role, PartyIds = new[] {new PartyId("id", "type")}};
        }
    }
}