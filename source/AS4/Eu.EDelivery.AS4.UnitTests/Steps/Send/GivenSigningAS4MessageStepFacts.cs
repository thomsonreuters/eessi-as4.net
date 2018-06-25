using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Moq;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SignAS4MessageStep" />
    /// </summary>
    public class GivenSigningAS4MessageStepFacts
    {
        [Fact]
        public async Task Doesnt_Sign_If_Message_Is_Empty()
        {
            // Arrange
            var context = new MessagingContext(AS4Message.Empty, MessagingContextMode.Unknown);

            // Act
            StepResult stepResult = await ExerciseSigning(context, sendPMode: null);

            // Assert
            AssertNotSignedSecurityHeader(stepResult);
        }

        [Fact]
        public async Task Doesnt_Sign_If_Response_PMode_Signing_Is_Disabled()
        {
            // Arrange
            MessagingContext context = AS4UserMessageWithAttachment();

            // Act
            StepResult stepResult = await ExerciseSigning(context, PModeWithoutSigningSettings());

            // Assert
            AssertNotSignedSecurityHeader(stepResult);
        }

        [Fact]
        public async Task Doesnt_Sign_If_Existing_Send_PMode_Signing_Is_Disabled()
        {
            // Arrange
            MessagingContext context = AS4UserMessageWithAttachment();
            context.SendingPMode = PModeWithoutSigningSettings();

            // Act
            StepResult stepResult = await ExerciseSigning(context, sendPMode: null);

            // Assert
            AssertNotSignedSecurityHeader(stepResult);
        }

        private static SendingProcessingMode PModeWithoutSigningSettings()
        {
            return new SendingProcessingMode { Security = { Signing = { IsEnabled = false } } };
        }

        private static void AssertNotSignedSecurityHeader(StepResult result)
        {
            SecurityHeader securityHeader = result.MessagingContext.AS4Message.SecurityHeader;

            Assert.False(securityHeader.IsSigned);
            Assert.False(securityHeader.IsEncrypted);
        }

        [Fact]
        public async Task Fails_To_Sign_Message_If_Certificate_Hasnt_Right_KeySet()
        {
            // Arrange
            var certWithoutPrivateKey = new X509Certificate2(AccessPointA, access_point_a_password, X509KeyStorageFlags.Exportable);

            SendingProcessingMode pmode = PModeWithSigningSettings();
            pmode.Security.Signing.SigningCertificateInformation = new CertificateFindCriteria
            {
                CertificateFindValue = "AccessPointA"
            };

            MessagingContext context = AS4UserMessageWithAttachment();

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => ExerciseSigning(context, pmode, certWithoutPrivateKey));
        }

        [Fact]
        public async Task Sign_Message_If_Response_PMode_Signing_Is_Enabled()
        {
            // Arrange
            MessagingContext context = AS4UserMessageWithAttachment();

            // Act
            StepResult result = await ExerciseSigning(context, PModeWithSigningSettings());

            // Assert
            Assert.True(result.MessagingContext.AS4Message.SecurityHeader.IsSigned);
        }

        [Fact]
        public async Task Sign_Message_If_Existing_PMode_Signing_Is_Enabled()
        {
            // Arrange
            MessagingContext context = AS4UserMessageWithAttachment();
            context.SendingPMode = PModeWithSigningSettings();

            // Act
            StepResult result = await ExerciseSigning(context, sendPMode: null);

            // Assert
            Assert.True(result.MessagingContext.AS4Message.SecurityHeader.IsSigned);
        }

        private static MessagingContext AS4UserMessageWithAttachment()
        {
            var as4Message = AS4Message.Create(new FilledUserMessage());
            as4Message.AddAttachment(new FilledAttachment());

            return new MessagingContext(as4Message, MessagingContextMode.Unknown);
        }

        private static SendingProcessingMode PModeWithSigningSettings()
        {
            return new SendingProcessingMode
            {
                Security =
                {
                    Signing =
                    {
                        IsEnabled = true,
                        KeyReferenceMethod = X509ReferenceType.BSTReference,
                        SigningCertificateInformation = new CertificateFindCriteria
                        {
                            CertificateFindValue = "PartyA",
                            CertificateFindType = X509FindType.FindBySubjectName
                        },
                        Algorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                        HashFunction = "http://www.w3.org/2001/04/xmlenc#sha256"
                    }
                }
            };
        }

        private static async Task<StepResult> ExerciseSigning(MessagingContext context, SendingProcessingMode sendPMode)
        {
            return await ExerciseSigning(context, sendPMode, r => { });
        }

        private static async Task<StepResult> ExerciseSigning(MessagingContext context, SendingProcessingMode sendPMode, X509Certificate2 certificate)
        {
            return await ExerciseSigning(context, sendPMode, r => r.CertificateStore.Add(certificate));
        }

        private static async Task<StepResult> ExerciseSigning(
            MessagingContext context,
            SendingProcessingMode sendPMode,
            Action<StubCertificateRepository> updateStore)
        {
            var stubCertRepo = new StubCertificateRepository();
            updateStore(stubCertRepo);

            var stubConfig = new Mock<IConfig>();
            stubConfig.Setup(c => c.GetReferencedSendingPMode(context.ReceivingPMode)).Returns(sendPMode);

            var sut = new SignAS4MessageStep(stubConfig.Object, stubCertRepo);
            return await sut.ExecuteAsync(context);
        }
    }
}