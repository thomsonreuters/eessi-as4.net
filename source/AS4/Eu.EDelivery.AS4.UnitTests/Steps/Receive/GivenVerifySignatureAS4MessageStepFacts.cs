using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="VerifySignatureAS4MessageStep"/>
    /// </summary>
    public class GivenVerifySignatureAS4MessageStepFacts
    {
        private const string ContentType = "multipart/related; boundary=\"=-dXYE+NJdacou7AbmYZgUPw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";
        private readonly Mock<ICertificateRepository> _mockedCertificateRespository;
        private readonly VerifySignatureAS4MessageStep _step;
        private InternalMessage _internalMessage;

        public GivenVerifySignatureAS4MessageStepFacts()
        {
            MapInitialization.InitializeMapper();

            this._mockedCertificateRespository = new Mock<ICertificateRepository>();
            this._step = new VerifySignatureAS4MessageStep(this._mockedCertificateRespository.Object);
        }

        /// <summary>
        /// Testing the Step with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                base._internalMessage = await base.GetSignedInternalMessageAsync(
                    Properties.Resources.as4_soap_signed_message);

                UsingAllowedSigningVerification();
                
                // Act
                StepResult result = await base._step
                    .ExecuteAsync(base._internalMessage, CancellationToken.None);
                
                // Assert
                Assert.NotNull(result);
                Assert.True(result.InternalMessage.AS4Message.IsSigned);
            }
        }

        /// <summary>
        /// Testing the Step with invalid arguments
        /// </summary>
        public class GivenInvalidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsAsync()
            {
                // Arrange
                base._internalMessage = await GetSignedInternalMessageAsync(
                    Properties.Resources.as4_soap_wrong_signed_message);

               UsingAllowedSigningVerification();
                
                // Act / Assert
                AS4Exception exception = await Assert.ThrowsAsync<AS4Exception>(() => 
                    base._step.ExecuteAsync(base._internalMessage, CancellationToken.None));
                Assert.Equal(ErrorCode.Ebms0101, exception.ErrorCode);
            }

            [Fact]
            public async Task ThenExecuteStepFailsWithUntrustedCertificateAsync()
            {
                // Arrange
                base._internalMessage = await GetSignedInternalMessageAsync(
                    Properties.Resources.as4_soap_untrusted_signed_message);

                UsingAllowedSigningVerification();

                // Act / Assert
                AS4Exception exception = await Assert.ThrowsAsync<AS4Exception>(()
                    => base._step.ExecuteAsync(base._internalMessage, CancellationToken.None));
                Assert.Equal(ErrorCode.Ebms0101, exception.ErrorCode);
            }
        }

        protected async Task<InternalMessage> GetSignedInternalMessageAsync(string xml)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var serializer = new SoapEnvelopeSerializer();
            AS4Message as4Message = await serializer
                .DeserializeAsync(memoryStream, ContentType, CancellationToken.None);

            return new InternalMessage(as4Message);
        }

        protected void UsingAllowedSigningVerification()
        {
            var receivingPMode = new ReceivingProcessingMode();
            receivingPMode.Security.SigningVerification.Signature = Limit.Allowed;
            this._internalMessage.AS4Message.ReceivingPMode = receivingPMode;
        }
    }
}
