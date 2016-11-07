using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive.Rules
{
    /// <summary>
    /// Testing <see cref="PModeUndefindPartyInfoRule"/>
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
                var rule = new PModeUndefindPartyInfoRule();
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
                var rule = new PModeUndefindPartyInfoRule();
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
                var rule = new PModeUndefindPartyInfoRule();
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
                var rule = new PModeUndefindPartyInfoRule();
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
                        PartyInfo = new PartyInfo {ToParty = CreateFilledParty(), FromParty = CreateFilledParty()}
                    }
                };
            }

            private Party CreateFilledParty()
            {
                return new Party(role: "role", partyId: new PartyId("party-id"));
            }
        }
    }
}