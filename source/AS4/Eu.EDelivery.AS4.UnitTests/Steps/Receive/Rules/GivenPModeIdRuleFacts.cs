using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive.Rules
{
    /// <summary>
    /// Testing <see cref="PModeIdRule"/>
    /// </summary>
    public class GivenPModeIdRuleFacts
    {
        public class GivenValidArugments : GivenPModeIdRuleFacts
        {
            [Theory, InlineData("pmode-id")]
            public void ThenRuleApply(string pmodeId)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id");
                userMessage.CollaborationInfo = new CollaborationInfo(new AgreementReference(string.Empty, pmodeId));
                var receivingPMode = new ReceivingProcessingMode {Id = pmodeId};
                var rule = new PModeIdRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(30, points);
            }

            [Theory, InlineData("pmode-id")]
            public void ThenRuleDontApply(string pmodeId)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id");
                var receivingPMode = new ReceivingProcessingMode { Id = pmodeId };
                var rule = new PModeIdRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }

        public class GivenInvalidArguments : GivenPModeIdRuleFacts
        {
            [Fact]
            public void ThenRuleDontApplyForNullAgreementRef()
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id")
                {
                    CollaborationInfo = {AgreementReference = null}
                };
                var receivingPMode = new ReceivingProcessingMode { Id = "pmode-id" };
                var rule = new PModeIdRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }
    }
}
