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
        [InlineData(200, 128)]
        public void ValidSendingPMode_IfKeySizeIs(int beforeKeySize, int afterKeySize)
        {
            // Arrange
            var sut = new SendingProcessingModeValidator();

            SendingProcessingMode pmode = new ValidSendingPModeFactory().Create();
            pmode.Security.Encryption.IsEnabled = true;
            pmode.Security.Encryption.AlgorithmKeySize = beforeKeySize;

            // Act
            sut.Validate(pmode);

            // Assert
            Assert.True(pmode.Security.Encryption.AlgorithmKeySize == afterKeySize);
        }

        [Fact]
        public void DynamicDiscoveryMustBeSpecified_WhenNoSendingConfiguration()
        {
            SendingProcessingMode pmode = new SendingProcessingMode
            {
                Id = "Test",
                MepBinding = MessageExchangePatternBinding.Pull,
                PullConfiguration = null,
                PushConfiguration = null,
                DynamicDiscovery = null
            };

            var validator = new SendingProcessingModeValidator();
            var result = validator.Validate(pmode);

            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public void PullConfigurationMustBeComplete_WhenMepBindingIsPull()
        {            
            var validator = new SendingProcessingModeValidator();

            SendingProcessingMode pmode = new SendingProcessingMode
            {
                Id = "Test",
                MepBinding = MessageExchangePatternBinding.Pull,
                PullConfiguration = new PullConfiguration(),
                PushConfiguration = null,
                DynamicDiscovery = null
            };

            var result = validator.Validate(pmode);

            Assert.False(result.IsValid);            
        }

        [Fact]
        public void SendConfigurationMayBeIncomplete_WhenDynamicDiscovery()
        {
            var validator = new SendingProcessingModeValidator();

            SendingProcessingMode pmode = new SendingProcessingMode
            {
                Id = "Test",
                MepBinding = MessageExchangePatternBinding.Pull,
                PullConfiguration = new PullConfiguration(),
                PushConfiguration = null,
                DynamicDiscovery = new DynamicDiscoveryConfiguration()
            };

            var result = validator.Validate(pmode);

            Assert.True(result.IsValid);

            pmode.PullConfiguration = null;
            pmode.MepBinding = MessageExchangePatternBinding.Push;

            result = validator.Validate(pmode);

            Assert.True(result.IsValid);
        }
    }
}