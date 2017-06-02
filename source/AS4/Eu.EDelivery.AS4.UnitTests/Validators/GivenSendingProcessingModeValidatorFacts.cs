using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="SendingProcessingModeValidator"/>
    /// TODO: return bool.
    /// </summary>
    public class GivenSendingProcessingModeValidatorFacts
    {
        [Theory]
        [InlineData(128)]
        [InlineData(192)]
        [InlineData(256)]
        public void ValidSendingPMode_IfKeySizeIs(int keysize)
        {
            // Arrange
            IValidator<SendingProcessingMode> sut = new SendingProcessingModeValidator();
            SendingProcessingMode pmode = new ValidStubSendingPModeFactory().Create();
            pmode.Security.Encryption.IsEnabled = true;
            pmode.Security.Encryption.AlgorithmKeySize = keysize;

            // Act
            sut.Validate(pmode);

            // Assert
            Assert.True(pmode.Security.Encryption.AlgorithmKeySize != 0);
        }

        [Fact]
        public void InvalidSendingPMode_IfInvalidKeySize()
        {
            // Arrange
            IValidator<SendingProcessingMode> sut = new SendingProcessingModeValidator();
            SendingProcessingMode pmode = new ValidStubSendingPModeFactory().Create();
            pmode.Security.Encryption.IsEnabled = true;
            pmode.Security.Encryption.AlgorithmKeySize = 200;

           // Act
            sut.Validate(pmode);

            // Assert
            Assert.True(pmode.Security.Encryption.AlgorithmKeySize == Encryption.Default.AlgorithmKeySize);
        }
    }
}
