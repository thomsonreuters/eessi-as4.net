using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="VerifySignatureAS4MessageStep" />
    /// </summary>
    public class GivenVerifySignatureAS4MessageStepFacts
    {
        private const string ContentType =
            "multipart/related; boundary=\"=-dXYE+NJdacou7AbmYZgUPw==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

        private readonly VerifySignatureAS4MessageStep _step;
        private MessagingContext _messagingContext;

        public GivenVerifySignatureAS4MessageStepFacts()
        {
            _step = new VerifySignatureAS4MessageStep();
        }

        public class GivenValidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsAsync()
            {
                // Arrange
                _messagingContext = await GetSignedInternalMessageAsync(Properties.Resources.as4_soap_signed_message);

                UsingAllowedSigningVerification();

                // Act
                StepResult result = await _step.ExecuteAsync(_messagingContext, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.True(result.MessagingContext.AS4Message.IsSigned);
            }
        }

        public class GivenInvalidArguments : GivenVerifySignatureAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepFailsAsync()
            {
                // Arrange
                _messagingContext =
                    await GetSignedInternalMessageAsync(Properties.Resources.as4_soap_wrong_signed_message);

                UsingAllowedSigningVerification();

                // Act
                StepResult result = await _step.ExecuteAsync(_messagingContext, CancellationToken.None);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0101, error.Code);
            }

            [Fact]
            public async Task ThenExecuteStepFailsWithUntrustedCertificateAsync()
            {
                // Arrange
                _messagingContext =
                    await GetSignedInternalMessageAsync(Properties.Resources.as4_soap_untrusted_signed_message);

                UsingAllowedSigningVerification();

                // Act
                StepResult result = await _step.ExecuteAsync(_messagingContext, CancellationToken.None);

                // Assert
                ErrorResult error = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0101, error.Code);
            }
        }

        protected async Task<MessagingContext> GetSignedInternalMessageAsync(string xml)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var serializer = new SoapEnvelopeSerializer();
            AS4Message as4Message = await serializer.DeserializeAsync(memoryStream, ContentType, CancellationToken.None);

            return new MessagingContext(as4Message, MessagingContextMode.Unknown);
        }

        protected void UsingAllowedSigningVerification()
        {
            var receivingPMode = new ReceivingProcessingMode();
            receivingPMode.Security.SigningVerification.Signature = Limit.Allowed;
            _messagingContext.ReceivingPMode = receivingPMode;
        }
    }
}