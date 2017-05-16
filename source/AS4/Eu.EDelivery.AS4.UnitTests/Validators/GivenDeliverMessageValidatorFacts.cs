using System;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Validators;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="DeliverMessageValidator" />
    /// </summary>
    public class GivenDeliverMessageValidatorFacts
    {
        public class GivenValidArguments : GivenDeliverMessageValidatorFacts
        {
            [Fact]
            public void ThenDeliverMessageIsValid()
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act
                validator.Validate(deliverMessage);
            }
        }

        public class GivenInvalidArguments : GivenDeliverMessageValidatorFacts
        {
            [Theory]
            [InlineData("message-id", null)]
            [InlineData(null, "mpc")]
            public void ThenDeliverMessageIsInvalidWithMissingMessageInfo(string messageId, string mpc)
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                deliverMessage.MessageInfo.MessageId = messageId;
                deliverMessage.MessageInfo.Mpc = mpc;
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(deliverMessage));
            }

            [Theory]
            [InlineData("type", null)]
            public void ThenDeliverMessageIsInvalidWithMissingService(string type, string value)
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                deliverMessage.CollaborationInfo.Service.Type = type;
                deliverMessage.CollaborationInfo.Service.Value = value;
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(deliverMessage));
            }

            [Theory]
            [InlineData(null)]
            public void ThenDeliverMessageIsInvalidWithMissingPModeId(string pmodeId)
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                deliverMessage.CollaborationInfo.AgreementRef.PModeId = pmodeId;
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(deliverMessage));
            }

            [Theory]
            [InlineData(null)]
            public void ThenDeliverMessageIsInvalidWithMissingAction(string action)
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                deliverMessage.CollaborationInfo.Action = action;
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(deliverMessage));
            }

            [Theory]
            [InlineData(null)]
            public void ThenDeliverMessageIsInvalidWithMissingConversationId(string conversationId)
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                deliverMessage.CollaborationInfo.ConversationId = conversationId;
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(deliverMessage));
            }

            [Theory]
            [InlineData("Sender", "Id", null)]
            [InlineData("Sender", null, "Type")]
            public void ThenDeliverMessageIsInvalidWithMissingFromParty(
                string partyRole,
                string partyId,
                string partyType)
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                deliverMessage.PartyInfo.FromParty.Role = partyRole;
                deliverMessage.PartyInfo.FromParty.PartyIds.First().Id = partyId;
                deliverMessage.PartyInfo.FromParty.PartyIds.First().Type = partyType;
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(deliverMessage));
            }

            [Theory]
            [InlineData("Receiver", "Id", null)]
            [InlineData("Receiver", null, "Type")]
            public void ThenDeliverMessageIsInvalidWithMissingToParty(
                string partyRole,
                string partyId,
                string partyType)
            {
                // Arrange
                DeliverMessage deliverMessage = CreateValidDeliverMessage();
                deliverMessage.PartyInfo.ToParty.Role = partyRole;
                deliverMessage.PartyInfo.ToParty.PartyIds.First().Id = partyId;
                deliverMessage.PartyInfo.ToParty.PartyIds.First().Type = partyType;
                IValidator<DeliverMessage> validator = new DeliverMessageValidator();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => validator.Validate(deliverMessage));
            }
        }

        protected DeliverMessage CreateValidDeliverMessage()
        {
            return new DeliverMessage
            {
                MessageInfo = CreateValidMessageInfo(),
                CollaborationInfo = CreateValidCollaborationInfo(),
                PartyInfo = CreateValidPartyInfo()
            };
        }

        private MessageInfo CreateValidMessageInfo()
        {
            return new MessageInfo {MessageId = Guid.NewGuid().ToString(), Mpc = Guid.NewGuid().ToString()};
        }

        private CollaborationInfo CreateValidCollaborationInfo()
        {
            return new CollaborationInfo
            {
                Action = Constants.Namespaces.TestAction,
                ConversationId = "1",
                Service = {
                             Type = "StoreServices", Value = Constants.Namespaces.TestService
                          },
                AgreementRef = {
                                  PModeId = "pmode-id"
                               }
            };
        }

        private PartyInfo CreateValidPartyInfo()
        {
            return new PartyInfo {FromParty = CreateValidParty("Sender"), ToParty = CreateValidParty("Receiver")};
        }

        private Party CreateValidParty(string role)
        {
            return new Party {Role = role, PartyIds = new[] {new PartyId("id", "type")}};
        }
    }
}