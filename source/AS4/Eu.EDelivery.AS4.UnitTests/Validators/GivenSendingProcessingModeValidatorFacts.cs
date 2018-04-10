using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using FsCheck;
using FsCheck.Xunit;
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

            SendingProcessingMode pmode = ValidSendingPModeFactory.Create();
            pmode.Security.Encryption.IsEnabled = true;
            pmode.Security.Encryption.AlgorithmKeySize = beforeKeySize;

            // Act
            ExerciseValidation(pmode);

            // Assert
            Assert.True(pmode.Security.Encryption.AlgorithmKeySize == afterKeySize);
        }

        [Fact]
        public void PushConfigurationMustNotBeSpecified_WhenPulling()
        {
            SendingProcessingMode pmode = new SendingProcessingMode
            {
                Id = "Test",
                MepBinding = MessageExchangePatternBinding.Pull,
                PushConfiguration = new PushConfiguration(),
                DynamicDiscovery = null
            };

            var result = ExerciseValidation(pmode);

            Assert.False(result.IsValid);

            pmode.PushConfiguration = null;

            result = ExerciseValidation(pmode);

            Assert.True(result.IsValid, result.AppendValidationErrorsToErrorMessage("Failed validation:"));
        }

        [Fact]
        public void SendConfigurationMayBeIncomplete_WhenDynamicDiscovery()
        {
            SendingProcessingMode pmode = new SendingProcessingMode
            {
                Id = "Test",
                MepBinding = MessageExchangePatternBinding.Pull,
                PushConfiguration = null,
                DynamicDiscovery = new DynamicDiscoveryConfiguration()
            };

            var result = ExerciseValidation(pmode);

            Assert.True(result.IsValid, result.AppendValidationErrorsToErrorMessage("Failed validation:"));
        }

        [Property]
        public Property Url_Should_Be_Present_When_SMP_Is_Disabled(string url)
        {
            var pmode = new SendingProcessingMode
            {
                Id = "ignored",
                DynamicDiscovery = new DynamicDiscoveryConfiguration {SmpProfile = null},
                PushConfiguration = new PushConfiguration {Protocol = {Url = url}}
            };

            var result = ExerciseValidation(pmode);

            bool urlPresent = url != null;
            return (result.IsValid == urlPresent).ToProperty();
        }

        private static ValidationResult ExerciseValidation(SendingProcessingMode pmode)
        {
            return SendingProcessingModeValidator.Instance.Validate(pmode);
        }
    }
}