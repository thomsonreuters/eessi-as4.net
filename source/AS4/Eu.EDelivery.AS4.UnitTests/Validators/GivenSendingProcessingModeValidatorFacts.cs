using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="SendingProcessingModeValidator"/>
    /// </summary>
    public class GivenSendingProcessingModeValidatorFacts
    {
        [Theory]
        [InlineData(128, 128)]
        [InlineData(192, 192)]
        [InlineData(256, 256)]
        [InlineData(200, 256)]
        public void ValidSendingPMode_IfKeySizeIs(int beforeKeySize, int afterKeySize)
        {
            // Arrange
            var sut = new SendingProcessingModeValidator();

            SendingProcessingMode pmode = new ValidStubSendingPModeFactory().Create();
            pmode.Security.Encryption.IsEnabled = true;
            pmode.Security.Encryption.AlgorithmKeySize = beforeKeySize;

            // Act
            sut.Validate(pmode);

            // Assert
            Assert.True(pmode.Security.Encryption.AlgorithmKeySize == afterKeySize);
        }
    }
}