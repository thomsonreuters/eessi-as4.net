using System;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using static Eu.EDelivery.AS4.Mappings.Submit.SubmitMessageAgreementResolver;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    public class GivenSubmitAgreementMapperFacts
    {
        public class GivenValidArguments : GivenSubmitAgreementMapperFacts
        {
            [Fact]
            public void ThenAgreementReferenceMapsCorrectllyByUsingPModeValues()
            {
                // Arrange
                var submitMessage = new SubmitMessage {PMode = GetPopulatedSendPMode()};

                // Act
                AgreementReference userMessageAgreementRef = ResolveAgreementReference(submitMessage).UnsafeGet;

                // Assert
                AS4.Model.PMode.AgreementReference pmodeAgreementRef =
                    submitMessage.PMode.MessagePackaging.CollaborationInfo.AgreementReference;

                Assert.Equal(pmodeAgreementRef.Value, userMessageAgreementRef.Value);
                Assert.Equal(Maybe.Just(pmodeAgreementRef.Type), userMessageAgreementRef.Type);
            }

            [Fact]
            public void ThenAgreementReferenceMapsCorrectllyWhenPModeAllowsOverride()
            {
                // Arrange
                SubmitMessage submitMessage = GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = true;

                // Act
                AgreementReference userMessageAgreement = ResolveAgreementReference(submitMessage).UnsafeGet;

                // Assert
                Agreement submitAgreementRef = submitMessage.Collaboration.AgreementRef;

                Assert.Equal(submitAgreementRef.Value, userMessageAgreement.Value);
                Assert.Equal(Maybe.Just(submitAgreementRef.RefType), userMessageAgreement.Type);
            }

            [Fact]
            public void ThenAgreementReferenceMapsCorrectlyByUsingDefaults()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PMode = new SendingProcessingMode {MessagePackaging = {CollaborationInfo = new AS4.Model.PMode.CollaborationInfo()}}
                };
                var userMessage = new UserMessage("message-id");

                // Act
                ResolveAgreementReference(submitMessage);

                // Assert
                Assert.Equal(Maybe<AS4.Model.Core.AgreementReference>.Nothing, userMessage.CollaborationInfo.AgreementReference);
            }
        }

        public class GivenInvalidArguments : GivenSubmitAgreementMapperFacts
        {
            [Fact]
            public void ThenAgreementReferenceMapsIncorrectllyWhenPModeDoesNotAllowsOverride()
            {
                // Arrange
                SubmitMessage submitMessage = GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = false;

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => ResolveAgreementReference(submitMessage));
            }

            [Fact]
            public void ThenAgreementReferenceMapsIncorrectllyWhenPModeDoesNotAllowsOverrideName()
            {
                // Arrange
                SubmitMessage submitMessage = GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = false;
                submitMessage.Collaboration.AgreementRef.RefType = null;

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => ResolveAgreementReference(submitMessage));
            }

            [Fact]
            public void ThenAgreementReferenceMapsIncorrectllyWhenPModeDoesNotAllowsOverrideType()
            {
                // Arrange
                SubmitMessage submitMessage = GetPopulatedSubmitMessage();
                submitMessage.PMode.AllowOverride = false;
                submitMessage.Collaboration.AgreementRef.Value = null;

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => ResolveAgreementReference(submitMessage));
            }
        }

        protected SubmitMessage GetPopulatedSubmitMessage()
        {
            return new SubmitMessage
            {
                Collaboration = {
                                   AgreementRef = CreateDefaultAgreement()
                                },
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
                    CollaborationInfo =
                        new AS4.Model.PMode.CollaborationInfo
                        {
                            AgreementReference = new AS4.Model.PMode.AgreementReference {Value = "pmode-name", Type = "pmode-type"}
                        }
                }
            };
        }
    }
}