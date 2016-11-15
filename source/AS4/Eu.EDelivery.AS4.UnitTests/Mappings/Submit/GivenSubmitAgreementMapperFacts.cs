using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitMessageAgreementMapper"/>
    /// </summary>
    public class GivenSubmitAgreementMapperFacts
    {
        public class GivenValidArguments : GivenSubmitAgreementMapperFacts
        {
            [Fact]
            public void ThenAgreementReferenceMapsCorrectllyByUsingPModeValues()
            {
                // Arrange
                var submitMessage = new SubmitMessage {PMode = base.GetPopulatedSendPMode()};
                var userMessage = new UserMessage(messageId: "message-id");
                // Act
                new SubmitMessageAgreementMapper().Map(submitMessage, userMessage);
                // Assert
                AgreementReference userMessageAgreementRef = userMessage.CollaborationInfo.AgreementReference;
                AgreementReference pmodeAgreementRef =
                    submitMessage.PMode.MessagePackaging.CollaborationInfo.AgreementReference;

                Assert.Equal(pmodeAgreementRef.Value, userMessageAgreementRef.Value);
                Assert.Equal(pmodeAgreementRef.Type, userMessageAgreementRef.Type);
            }

            [Fact]
            public void ThenAgreementReferenceMapsCorrectllyWhenPModeAllowsOverride()
            {
                // Arrange
                SubmitMessage submitMessage = base.GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = true;
                var userMessage = new UserMessage(messageId: "message-id");
                // Act
                new SubmitMessageAgreementMapper().Map(submitMessage, userMessage);
                // Assert
                AgreementReference userMessageAgreement = userMessage.CollaborationInfo.AgreementReference;
                Agreement submitAgreementRef = submitMessage.Collaboration.AgreementRef;

                Assert.Equal(submitAgreementRef.Value, userMessageAgreement.Value);
                Assert.Equal(submitAgreementRef.RefType, userMessageAgreement.Type);
            }

            [Fact]
            public void ThenAgreementReferenceMapsCorrectlyByUsingDefaults()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PMode = new SendingProcessingMode {MessagePackaging = {CollaborationInfo = new CollaborationInfo()}}
                };
                var userMessage = new UserMessage(messageId: "message-id");
                // Act
                new SubmitMessageAgreementMapper().Map(submitMessage, userMessage);
                // Assert
                AgreementReference userMessageAgreementRef = userMessage.CollaborationInfo.AgreementReference;
                Assert.Null(userMessageAgreementRef.Value);
                Assert.Null(userMessageAgreementRef.Type);
            }
        }

        public class GivenInvalidArguments : GivenSubmitAgreementMapperFacts
        {
            [Fact]
            public void ThenAgreementReferenceMapsIncorrectllyWhenPModeDoesNotAllowsOverride()
            {
                // Arrange
                SubmitMessage submitMessage = base.GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = false;
                var userMessage = new UserMessage(messageId: "message-id");
                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => new SubmitMessageAgreementMapper().Map(submitMessage, userMessage));
            }

            [Fact]
            public void ThenAgreementReferenceMapsIncorrectllyWhenPModeDoesNotAllowsOverrideType()
            {
                // Arrange
                SubmitMessage submitMessage = base.GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = false;
                submitMessage.Collaboration.AgreementRef.Value = null;

                var userMessage = new UserMessage(messageId: "message-id");
                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => new SubmitMessageAgreementMapper().Map(submitMessage, userMessage));
            }

            [Fact]
            public void ThenAgreementReferenceMapsIncorrectllyWhenPModeDoesNotAllowsOverrideName()
            {
                // Arrange
                SubmitMessage submitMessage = base.GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = false;
                submitMessage.Collaboration.AgreementRef.RefType = null;

                var userMessage = new UserMessage(messageId: "message-id");
                // Act / Assert
                Assert.Throws<AS4Exception>(
                    () => new SubmitMessageAgreementMapper().Map(submitMessage, userMessage));
            }
        }

        protected SubmitMessage GetPopulatedSubmitMessage()
        {
            return new SubmitMessage
            {
                Collaboration = {AgreementRef = CreateDefaultAgreement()},
                PMode = GetPopulatedSendPMode()
            };
        }

        protected Agreement CreateDefaultAgreement()
        {
            return new Agreement {RefType = "submit-type", Value = "submit-value"};
        }

        protected SendingProcessingMode GetPopulatedSendPMode()
        {
            return new SendingProcessingMode
            {
                MessagePackaging =
                {
                    CollaborationInfo = new CollaborationInfo {AgreementReference = CreateDefaultAgreementReference()}
                }
            };
        }

        private AgreementReference CreateDefaultAgreementReference()
        {
            return new AgreementReference {Value = "pmode-name", Type = "pmode-type"};
        }
    }
}