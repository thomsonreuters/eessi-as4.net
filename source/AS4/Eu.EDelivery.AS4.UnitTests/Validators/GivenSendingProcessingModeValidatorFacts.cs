using System;
using System.Linq;
using Eu.EDelivery.AS4.Extensions;
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

        [Property]
        public Property RetryReliability_Should_Be_Present_When_IsEnabled(
            bool isEnabled,
            int retryCount,
            TimeSpan retryInterval)
        {
            return new Func<SendingProcessingMode, RetryReliability>[]
            {
                p => p.ReceiptHandling.Reliability,
                p => p.ErrorHandling.Reliability,
                p => p.ExceptionHandling.Reliability
            }
            .Select(f => TestRelialityForEnabledFlag(isEnabled, retryCount, retryInterval, f))
            .Aggregate((p1, p2) => p1.And(p2));
        }

        private static Property TestRelialityForEnabledFlag(
            bool isEnabled,
            int retryCount,
            TimeSpan retryInterval,
            Func<SendingProcessingMode, RetryReliability> getReliability)
        {
            return Prop.ForAll(
                Gen.Frequency(
                       Tuple.Create(2, Gen.Constant(retryInterval.ToString())),
                       Tuple.Create(1, Arb.From<string>().Generator))
                   .ToArbitrary(),
                retryIntervalText =>
                {
                    // Arrange
                    SendingProcessingMode pmode = ValidSendingPModeFactory.Create();
                    RetryReliability r = getReliability(pmode);
                    r.IsEnabled = isEnabled;
                    r.RetryCount = retryCount;
                    r.RetryInterval = retryIntervalText;

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool correctConfigured =
                        retryCount > 0
                        && r.RetryInterval.AsTimeSpan() > default(TimeSpan);

                    bool expected =
                        !isEnabled && !correctConfigured
                        || !isEnabled
                        || correctConfigured;

                    return expected.Equals(result.IsValid)
                        .Label(result.AppendValidationErrorsToErrorMessage(string.Empty))
                        .Classify(result.IsValid, "Valid PMode")
                        .Classify(!result.IsValid, "Invalid PMode")
                        .Classify(correctConfigured, "Correct Reliability")
                        .Classify(!correctConfigured, "Incorrect Reliability")
                        .Classify(isEnabled, "Reliability is enabled")
                        .Classify(!isEnabled, "Reliability is disabled");
                });
        }

        private static ValidationResult ExerciseValidation(SendingProcessingMode pmode)
        {
            return SendingProcessingModeValidator.Instance.Validate(pmode);
        }
    }
}