using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using Service = Eu.EDelivery.AS4.Model.PMode.Service;

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

                userMessage.CollaborationInfo = new CollaborationInfo(
                    userMessage.CollaborationInfo.AgreementReference,
                    userMessage.CollaborationInfo.Service,
                    action: String.Empty,
                    conversationId: CollaborationInfo.DefaultConversationId);

                var rule = new PModeServiceActionRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
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
                        Service = new Service() {Value = serviceName, Type = serviceType}
                    }
                }
            };
        }

        protected UserMessage CreateServiceActionUserMesage(string serviceName, string serviceType, string action)
        {
            return new UserMessage(messageId: "message-id")
            {
                CollaborationInfo = new CollaborationInfo(
                    Maybe<AgreementReference>.Nothing,
                    new AS4.Model.Core.Service(serviceName, serviceType),
                    action,
                    CollaborationInfo.DefaultConversationId)
            };
        }
    }
}
