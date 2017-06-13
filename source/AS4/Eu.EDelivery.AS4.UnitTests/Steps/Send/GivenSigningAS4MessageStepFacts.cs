using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Extensions.AS4MessageExtensions;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SignAS4MessageStep" />
    /// </summary>
    public class GivenSigningAS4MessageStepFacts
    {
        [Fact]
        public async Task DoesntSignMessage_IfAS4MessageIsEmpty()
        {
            // Arrange
            MessagingContext context = AS4MessageContext(new AS4MessageBuilder().Build(), pmode: null);

            // Act
            StepResult stepResult = await ExerciseSigning(context);

            // Assert
            AssertNotSignedSecurityHeader(stepResult);
        }

        [Fact]
        public async Task DoesntSignMessage_IfPModeIsNotSetForSigning()
        {
            // Arrange
            MessagingContext context = AS4MessageContext(AS4UserMessageWithAttachment(), PModeWithoutSigningSettings());

            // Act
            StepResult stepResult = await ExerciseSigning(context);

            // Assert
            AssertNotSignedSecurityHeader(stepResult);
        }

        private static SendingProcessingMode PModeWithoutSigningSettings()
        {
            return new SendingProcessingMode {Security = {Signing = {IsEnabled = false}}};
        }

        private static void AssertNotSignedSecurityHeader(StepResult result)
        {
            SecurityHeader securityHeader = result.MessagingContext.AS4Message.SecurityHeader;

            Assert.False(securityHeader.IsSigned);
            Assert.False(securityHeader.IsEncrypted);
        }

        [Fact]
        public async Task FailToSignMessage_IfCertificateHasntRightKeySet()
        {
            // Arrange
            var certWithoutPrivateKey = new X509Certificate2(AccessPointA, access_point_a_password, X509KeyStorageFlags.Exportable);

            SendingProcessingMode pmode = PModeWithSigningSettings();
            pmode.Security.Signing.PrivateKeyFindValue = "AccessPointA";

            MessagingContext context = AS4MessageContext(AS4UserMessageWithAttachment(), pmode);

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => ExerciseSigning(context, certWithoutPrivateKey));
        }

        [Fact]
        public async Task SignMessage_IfPModeIsSetForSigning()
        {
            // Arrange
            MessagingContext context = AS4MessageContext(AS4UserMessageWithAttachment(), PModeWithSigningSettings());
            [Fact]
            public async Task ThenMessageDontGetSignedWhenItsDisabledAsync()
            {
                // Arrange
                var internalMessage = new MessagingContext(AS4Message.Empty)
                {
                    SendingPMode = new SendingProcessingMode()
                };

                internalMessage.SendingPMode.Security.Signing.IsEnabled = false;

            // Act
            StepResult result = await ExerciseSigning(context);

            // Assert
            SecurityHeader securityHeader = result.MessagingContext.AS4Message.SecurityHeader;

            Assert.True(securityHeader.IsSigned);
        }

        private static AS4Message AS4UserMessageWithAttachment()
        {
            var as4Message = AS4Message.Create(new FilledUserMessage());
            as4Message.AddAttachment(new FilledAttachment());

            return as4Message;
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
                        PrivateKeyFindValue = "PartyA",
                        PrivateKeyFindType = X509FindType.FindBySubjectName,
                        Algorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                        HashFunction = "http://www.w3.org/2001/04/xmlenc#sha256"
                    }
                }
            };
        }

        private static MessagingContext AS4MessageContext(AS4Message as4Message, SendingProcessingMode pmode)
        {
            return new MessagingContext(as4Message) {SendingPMode = pmode};
        }

        private static async Task<StepResult> ExerciseSigning(MessagingContext context)
        {
            return await ExerciseSigning(context, r => { });
        }

        private static async Task<StepResult> ExerciseSigning(MessagingContext context, X509Certificate2 certificate)
        {
            return await ExerciseSigning(context, r => r.CertificateStore.Add(certificate));
        }

        private static async Task<StepResult> ExerciseSigning(
            MessagingContext context,
            Action<StubCertificateRepository> updateStore)
        {
            var stub = new StubCertificateRepository();
            updateStore(stub);

            var sut = new SignAS4MessageStep(stub);
            return await sut.ExecuteAsync(context, CancellationToken.None);
        }
    }
}