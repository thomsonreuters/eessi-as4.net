using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive.Rules
{
    /// <summary>
    /// Testing <see cref="PModeAgreementRefRule"/>
    /// </summary>
    public class GivenPModeAgreementRefRuleFacts
    {
        public class GivenValidArguments : GivenPModeAgreementRefRuleFacts
        {
            [Theory, InlineData("name", "type")]
            public void ThenRuleApply(string name, string type)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id");
                userMessage.CollaborationInfo.AgreementReference.Value = name;
                userMessage.CollaborationInfo.AgreementReference.Type = type;

                ReceivingProcessingMode receivingPMode =
                    base.CreateAgreementRefReceivingPMode(name, type);

                var rule = new PModeAgreementRefRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(4, points);
            }

            [Theory, InlineData("name", "type")]
            public void ThenRuleDontApplyForName(string name, string type)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id");
                userMessage.CollaborationInfo.AgreementReference.Value = "not-equal";
                userMessage.CollaborationInfo.AgreementReference.Type = type;

                ReceivingProcessingMode receivingPMode =
                    CreateAgreementRefReceivingPMode(name, type);

                var rule = new PModeAgreementRefRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }

            [Theory, InlineData("name", "type")]
            public void ThenRuleDontApplyForType(string name, string type)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id");
                userMessage.CollaborationInfo.AgreementReference.Value = name;
                userMessage.CollaborationInfo.AgreementReference.Type = "not-equal";

                ReceivingProcessingMode receivingPMode =
                    CreateAgreementRefReceivingPMode(name, type);

                var rule = new PModeAgreementRefRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }

        public class GivenInvalidArguments : GivenPModeAgreementRefRuleFacts
        {
            [Theory, InlineData("name", "type")]
            public void ThenRuleDontApplyForNullPModeAgreementRef(string name, string type)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id");
                userMessage.CollaborationInfo.AgreementReference.Value = name;
                userMessage.CollaborationInfo.AgreementReference.Type = type;

                var receivingPMode = new ReceivingProcessingMode
                {
                    MessagePackaging = {CollaborationInfo = new CollaborationInfo {AgreementReference = null}}
                };

                var rule = new PModeAgreementRefRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }

            [Theory, InlineData("name", "type")]
            public void ThenRuleDontApplyForNullPModeCollaborationInfo(string name, string type)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id");
                userMessage.CollaborationInfo.AgreementReference.Value = name;
                userMessage.CollaborationInfo.AgreementReference.Type = type;

                var receivingPMode = new ReceivingProcessingMode
                {
                    MessagePackaging = {CollaborationInfo = null}
                };

                var rule = new PModeAgreementRefRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }

            [Theory, InlineData("name", "type")]
            public void ThenRuleDontApplyForNullUserMessageAgreementRef(string name, string type)
            {
                // Arrange
                var userMessage = new UserMessage(messageId: "message-id")
                {
                    CollaborationInfo = {AgreementReference = null}
                };
                ReceivingProcessingMode receivingPMode = 
                    base.CreateAgreementRefReceivingPMode(name, type);

                var rule = new PModeAgreementRefRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }

        protected ReceivingProcessingMode CreateAgreementRefReceivingPMode(string name, string type)
        {
            return new ReceivingProcessingMode
            {
                MessagePackaging =
                {
                    CollaborationInfo = new AS4.Model.PMode.CollaborationInfo {AgreementReference = {Value = name, Type = type}}
                }
            };
        }
    }
}