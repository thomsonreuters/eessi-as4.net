using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;
using Party = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive.Rules
{
    /// <summary>
    /// Testing <see cref="PModeUndefinedPartyInfoRule"/>
    /// </summary>
    public class GivenPModeUndefinedPartyInfoRuleFacts
    {
        public class GivenValidArguments : GivenPModeUndefinedPartyInfoRuleFacts
        {
            [Fact]
            public void ThenRuleApplyForEmptyPartyInfo()
            {
                // Arrange
                var receivingPMode = new ReceivingProcessingMode {MessagePackaging = {PartyInfo = new PartyInfo()}};
                var userMessage = new UserMessage(messageId: "message-id");
                var rule = new PModeUndefinedPartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(7, points);
            }

            [Fact]
            public void ThenRuleApplyForNullPartyInfo()
            {
                // Arrange
                var receivingPMode = new ReceivingProcessingMode {MessagePackaging = {PartyInfo = null}};
                var userMessage = new UserMessage(messageId: "message-id");
                var rule = new PModeUndefinedPartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(7, points);
            }

            [Fact]
            public void ThenRuleApplyForEmptyParties()
            {
                // Arrange
                var receivingPMode = new ReceivingProcessingMode
                {
                    MessagePackaging = {PartyInfo = new PartyInfo {ToParty = new Party(), FromParty = new Party()}}
                };
                var userMessage = new UserMessage(messageId: "message-id");
                var rule = new PModeUndefinedPartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(7, points);
            }

            [Fact]
            public void ThenRuleDontApplyForFilledParties()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode = CreateFilledPartiesReceivingPMode();
                var userMessage = new UserMessage(messageId: "message-id");
                var rule = new PModeUndefinedPartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }

            private ReceivingProcessingMode CreateFilledPartiesReceivingPMode()
            {
                return new ReceivingProcessingMode
                {
                    MessagePackaging =
                    {
                        PartyInfo = new PartyInfo
                        {
                            ToParty = CreateFilledParty(),
                            FromParty = CreateFilledParty()
                        }
                    }
                };
            }

            private static Party CreateFilledParty()
            {
                return new Party
                {
                    Role = "role",
                    PartyIds = { new PartyId { Id = "party-id" } }
                };
            }
        }
    }
}