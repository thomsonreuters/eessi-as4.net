using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <see cref="SignAS4MessageStep" />
    /// </summary>
    public class GivenSigningAS4MessageStepFacts
    {
        private readonly SignAS4MessageStep _step;
        private AS4Message _message;

        public GivenSigningAS4MessageStepFacts()
        {
            var mockedContext = new Mock<IConfig>();
            CreateAS4Message();

            _step = new SignAS4MessageStep(new CertificateRepository(mockedContext.Object));
        }

        private void CreateAS4Message()
        {
            _message = new AS4Message
            {
                SendingPMode =
                    new SendingProcessingMode {Security = new AS4.Model.PMode.Security {Signing = new Signing()}}
            };
        }

        /// <summary>
        /// Testing if the SigningTransmitter succeeds for the "Execute" Method
        /// </summary>
        public class GivenValidArgumentsExecute : GivenSigningAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenMessageDontGetSignedWhenItsDisabledAsync()
            {
                // Arrange
                _message.SendingPMode.Security.Signing.IsEnabled = false;
                var internalMessage = new InternalMessage(_message);

                // Act
                StepResult stepResult = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                SecurityHeader securityHeader = stepResult.InternalMessage.AS4Message.SecurityHeader;
                Assert.NotNull(securityHeader);
                Assert.False(securityHeader.IsSigned);
                Assert.False(securityHeader.IsEncrypted);
            }
        }

        /// <summary>
        /// Testing if the SigningTransmitter fails
        /// </summary>
        public class GivenSigningStepFails : GivenSigningAS4MessageStepFacts
        {
            [Fact]
            public void ThenConfigureTransmitterFails()
            {
                // Arrange

                // Act

                // Assert
            }

            [Fact]
            public void ThenTransmitMessageFails()
            {
                // Arrange

                // Act

                // Assert
            }
        }
    }
}