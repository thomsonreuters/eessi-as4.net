using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive.Rules
{
    /// <summary>
    /// Testing <see cref="PModePartyInfoRule"/>
    /// </summary>
    public class GivenPModePartyInfoRuleFacts
    {
        public class GivenValidArguments : GivenPModePartyInfoRuleFacts
        {
            [Theory, InlineData("role", "party-id")]
            public void ThenMaxPointsWhenPartyIdAndRoleMatch(string role, string partyId)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(CreateParty(role, partyId), CreateParty(role, partyId));
                UserMessage userMessage = CreateUserMessageWithParties(CreateParty(role, partyId), CreateParty(role, partyId));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(16, points);
            }

            [Fact]
            public void Then15PointsWhenBothSenderAndReceiverMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(CreateParty("senderrole1", "sender-id"), CreateParty("receiverole1", "receiver-id"));

                UserMessage userMessage = CreateUserMessageWithParties(CreateParty("", "sender-id"), CreateParty("", "receiver-id"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(15, points);
            }

            [Fact]
            public void Then8PointsWhenOnlyReceiverIdIsSpecifiedAndMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(null, CreateParty("receiverrole", "receiver"));

                UserMessage userMessage = CreateUserMessageWithParties(CreateParty("senderrole2", "senderId2"), CreateParty("", "receiver"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(8, points);
            }

            [Fact]
            public void Then7PointsWhenOnlySenderIdIsSpecifiedAndMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(CreateParty("sender-role1", "sender-id1"), null);

                UserMessage userMessage = CreateUserMessageWithParties(CreateParty("", "sender-id1"), CreateParty("", "receiver-id2"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(7, points);
            }

            [Fact]
            public void Then1PointWhenOnlyRoleMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(CreateParty("sender-role", "sender-1"), CreateParty("receiving-role", "receiver-1"));

                UserMessage userMessage = CreateUserMessageWithParties(CreateParty("sender-role", "sender-2"), CreateParty("receiving-role", "receiver-2"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(1, points);
            }

            [Fact]
            public void ThenZeroPointsWhenPartyIdAndRoleDontMatch()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(CreateParty("sender-role-1", "sender-1"), CreateParty("receiving-role-1", "receiver-1"));


                UserMessage userMessage =
                    CreateUserMessageWithParties(CreateParty("sender-role-2", "sender-2"), CreateParty("receiving-role-2", "receiver-2"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }
      
        protected UserMessage CreateUserMessageWithParties(Party fromParty, Party toParty)
        {
            return new UserMessage(messageId: "message-id")
            {
                Receiver = toParty,
                Sender = fromParty
            };
        }

        protected ReceivingProcessingMode CreateReceivingPModeWithParties(Party fromParty, Party toParty)
        {
            return new ReceivingProcessingMode
            {
                MessagePackaging =
                {
                    PartyInfo = new PartyInfo
                    {
                        FromParty = fromParty,
                        ToParty = toParty
                    }
                }
            };
        }

        private static Party CreateParty(string role, string partyId)
        {
            return new Party
            {
                Role = role,
                PartyIds = new List<PartyId> { new PartyId(partyId) }
            };
        }
    }
}