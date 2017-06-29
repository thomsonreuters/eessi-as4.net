using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
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
            
            SendingProcessingMode pmode = new ValidSendingPModeFactory().Create();
            pmode.Security.Encryption.IsEnabled = true;
            pmode.Security.Encryption.AlgorithmKeySize = beforeKeySize;

            // Act
            ExerciseValidation(pmode);

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

            var result = ExerciseValidation(pmode);

            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public void PullConfigurationMustBeComplete_WhenMepBindingIsPull()
        {            
            SendingProcessingMode pmode = new SendingProcessingMode
            {
                Id = "Test",
                MepBinding = MessageExchangePatternBinding.Pull,
                PullConfiguration = new PullConfiguration(),
                PushConfiguration = null,
                DynamicDiscovery = null
            };

            var result = ExerciseValidation(pmode);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void SendConfigurationMayBeIncomplete_WhenDynamicDiscovery()
        {         
            SendingProcessingMode pmode = new SendingProcessingMode
            {
                Id = "Test",
                MepBinding = MessageExchangePatternBinding.Pull,
                PullConfiguration = new PullConfiguration(),
                PushConfiguration = null,
                DynamicDiscovery = new DynamicDiscoveryConfiguration()
            };

            var result = ExerciseValidation(pmode);

            Assert.True(result.IsValid);

            pmode.PullConfiguration = null;
            pmode.MepBinding = MessageExchangePatternBinding.Push;

            result = ExerciseValidation(pmode);

            Assert.True(result.IsValid);
        }

        private static ValidationResult ExerciseValidation(SendingProcessingMode pmode)
        {
            var sut = new SendingProcessingModeValidator();
            return sut.Validate(pmode);
        }
    }
}