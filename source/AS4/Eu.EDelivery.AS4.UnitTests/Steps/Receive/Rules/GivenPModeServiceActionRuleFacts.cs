using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive.Rules
{
    /// <summary>
    /// Testing <see cref="PModeServiceActionRule"/>
    /// </summary>
    public class GivenPModeServiceActionRuleFacts
    {
        public class GivenValidArguments : GivenPModeServiceActionRuleFacts
        {
            [Theory, InlineData("serviceName", "serviceType", "action")]
            public void ThenRuleApply(string serviceName, string serviceType, string action)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode = 
                    base.CreateServiceActionReceivingPMode(serviceName, serviceType, action);
                receivingPMode.MessagePackaging.CollaborationInfo.Action = action;
                UserMessage userMessage = CreateServiceActionUserMesage(serviceName, serviceType, action);
                var rule = new PModeServiceActionRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(3, points);
            }

            [Theory, InlineData("serviceName", "serviceType", "action")]
            public void ThenRuleDontApply(string serviceName, string serviceType, string action)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode = 
                    CreateServiceActionReceivingPMode(serviceName, serviceType, action);
                receivingPMode.MessagePackaging.CollaborationInfo.Service.Value = "not-equal";

                UserMessage userMessage = CreateServiceActionUserMesage(serviceName, serviceType, action);
                var rule = new PModeServiceActionRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }

        public class GivenInvalidArguments : GivenPModeServiceActionRuleFacts
        {
            [Theory, InlineData("serviceName", "serviceType", "action")]
            public void ThenRuleDontApplyForNullPModeService(string serviceName, string serviceType, string action)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode = 
                    base.CreateServiceActionReceivingPMode(serviceName, serviceType, action);
                receivingPMode.MessagePackaging.CollaborationInfo.Service = null;

                UserMessage userMessage = CreateServiceActionUserMesage(serviceName, serviceType, action);
                var rule = new PModeServiceActionRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }

            [Theory, InlineData("serviceName", "serviceType", "action")]
            public void ThenRuleDontApplyForNullUserMessageAction(string serviceName, string serviceType, string action)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    base.CreateServiceActionReceivingPMode(serviceName, serviceType, action);
                UserMessage userMessage = CreateServiceActionUserMesage(serviceName, serviceType, action);
                userMessage.CollaborationInfo.Action = null;
                var rule = new PModeServiceActionRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }

        protected Service CreateService(string name, string type)
        {
            return new Service() {Value = name, Type = type};
        }

        protected ReceivingProcessingMode CreateServiceActionReceivingPMode(
            string serviceName, string serviceType, string action)
        {
            return new ReceivingProcessingMode
            {
                MessagePackaging =
                {
                    CollaborationInfo = new AS4.Model.PMode.CollaborationInfo()
                    {
                        Action = action,
                        Service = CreateService(serviceName, serviceType)
                    }
                }
            };
        }

        protected UserMessage CreateServiceActionUserMesage(string serviceName, string serviceType, string action)
        {
            return new UserMessage(messageId: "message-id")
            {
                CollaborationInfo =
                {
                    Action = action,
                    Service = CreateService(serviceName, serviceType)
                }
            };
        }
    }
}
