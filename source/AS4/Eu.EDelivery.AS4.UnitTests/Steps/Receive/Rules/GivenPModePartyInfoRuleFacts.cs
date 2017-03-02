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
                    CreateReceivingPModeWithParties(role, partyId);
                UserMessage userMessage = CreateUserMessageWithParties(role, partyId);

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(16, points);
            }

            [Theory, InlineData("role", "party-id")]
            public void Then15PointsWhenOnlyPartyIdMatches(string role, string partyId)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode = 
                    CreateReceivingPModeWithParties(role, partyId);
                receivingPMode.MessagePackaging.PartyInfo.FromParty.Role = "not-equal";
                UserMessage userMessage = CreateUserMessageWithParties(role, partyId);

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(15, points);
            }

            [Theory, InlineData("role", "party-id")]
            public void Then1PointWhenOnlyRoleMatches(string role, string partyId)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(role, partyId);
                receivingPMode.MessagePackaging.PartyInfo.FromParty.PartyIds.First().Id = "not-equal";
                UserMessage userMessage = CreateUserMessageWithParties(role, partyId);

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(1, points);
            }

            [Theory, InlineData("role", "party-id")]
            public void ThenZeroPointsWhenPartyIdAndRoleDontMatch(string role, string partyId)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(role, partyId);
                receivingPMode.MessagePackaging.PartyInfo.FromParty.PartyIds.First().Id = "not-equal";
                receivingPMode.MessagePackaging.PartyInfo.FromParty.Role = "not-equal";
                UserMessage userMessage = CreateUserMessageWithParties(role, partyId);

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }

        public class GivenInvalidArguments : GivenPModePartyInfoRuleFacts
        {
            [Theory, InlineData("role", "party-id")]
            public void ThenRuleDontApplyWithNullReceiver(string role, string partyId)
            {
                // Arrange
                UserMessage userMessage = base.CreateUserMessageWithParties(role, partyId);
                userMessage.Receiver = null;
                ReceivingProcessingMode receivingPMode = base.CreateReceivingPModeWithParties(role, partyId);
                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }

            [Theory, InlineData("role", "party-id")]
            public void ThenRuleDontApplyWithNullFromParty(string role, string partyId)
            {
                // Arrange
                UserMessage userMessage = base.CreateUserMessageWithParties(role, partyId);
                ReceivingProcessingMode receivingPMode = base.CreateReceivingPModeWithParties(role, partyId);
                receivingPMode.MessagePackaging.PartyInfo.FromParty = null;
                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }

        protected UserMessage CreateUserMessageWithParties(string role, string partyId)
        {
            return new UserMessage(messageId: "message-id")
            {
                Receiver = CreateParty(role, partyId),
                Sender = CreateParty(role, partyId)
            };
        }

        protected ReceivingProcessingMode CreateReceivingPModeWithParties(string role, string partyId)
        {
            return new ReceivingProcessingMode
            {
                MessagePackaging =
                {
                    PartyInfo = new PartyInfo
                    {
                        FromParty = CreateParty(role, partyId),
                        ToParty = CreateParty(role, partyId)
                    }
                }
            };
        }

        public Party CreateParty(string role, string partyId)
        {
            return new Party
            {
                Role = role,
                PartyIds = new List<PartyId> {new PartyId(partyId)}
            };
        }
    }
}