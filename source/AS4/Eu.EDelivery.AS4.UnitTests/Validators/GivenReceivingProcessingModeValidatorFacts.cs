using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    /// <summary>
    /// Testing <see cref="ReceivingProcessingModeValidator"/>
    /// </summary>
    public class GivenReceivingProcessingModeValidatorFacts
    {
        [Fact]
        public void PModeIsValid()
        {
            TestReceivePModeValidationSuccess(pmode => { });
        }

        [Fact]
        public void ForwardPModeIsValid()
        {
            TestReceivePModeValidationSuccess(
                pmode => { pmode.MessageHandling.Item = new Forward() { SendingPMode = "SomePModeId" }; });
        }

        [Fact]
        public void ForwardPModeIsValid_EvenIfNoReplyHandlingIsPresent()
        {
            TestReceivePModeValidationSuccess(pmode =>
            {
                pmode.ReplyHandling = null;
                pmode.MessageHandling.Item = new Forward { SendingPMode = "somepmodeid" };
            });
        }

        [Fact]
        public void ForwardPModeIsValid_EvenIfReplySendingPModeIsMissing()
        {
            TestReceivePModeValidationSuccess(pmode =>
            {
                pmode.ReplyHandling.SendingPMode = null;
                pmode.MessageHandling.Item = new Forward { SendingPMode = "somepmodeid" };
            });
        }

        [Fact]
        public void DeliverPModeIsNotValid_IfReplyHandlingIsMissing()
        {
            TestReceivePModeValidationFailure(pmode =>
            {
                pmode.ReplyHandling = null;
                pmode.MessageHandling.Item = new Deliver
                {
                    IsEnabled = false,
                    DeliverMethod = new Method(),
                    PayloadReferenceMethod = new Method()
                };
            });
        }

        [Fact]
        public void DeliverPModeIsNotValid_IfReplySendingPModeIsMissing()
        {
            TestReceivePModeValidationFailure(pmode =>
            {
                pmode.ReplyHandling.SendingPMode = null;
                pmode.MessageHandling.Item = new Deliver
                {
                    IsEnabled = false,
                    DeliverMethod = new Method(),
                    PayloadReferenceMethod = new Method()
                };
            });
        }

        [Property]
        public Property DeliverReliability_Is_Required_On_IsEnabled_Flag(
            bool isEnabled,
            int retryCount,
            TimeSpan retryInterval)
        {
            return TestRelialityForEnabledFlag(
                isEnabled,
                retryCount,
                retryInterval,
                pmode => pmode.MessageHandling.DeliverInformation.Reliability);
        }

        [Property]
        public Property ExceptionReliability_Is_Required_On_IsEnabled_Flag(
            bool isEnabled,
            int retryCount,
            TimeSpan retryInterval)
        {
            return TestRelialityForEnabledFlag(
                isEnabled,
                retryCount,
                retryInterval,
                p => p.ExceptionHandling.Reliability);
        }

        private static Property TestRelialityForEnabledFlag(
            bool isEnabled, 
            int retryCount, 
            TimeSpan retryInterval,
            Func<ReceivingProcessingMode, RetryReliability> getReliability)
        {
            return Prop.ForAll(
                Gen.Frequency(
                       Tuple.Create(2, Gen.Constant(retryInterval.ToString())),
                       Tuple.Create(1, Arb.From<string>().Generator))
                   .ToArbitrary(),
                retryIntervalText =>
                {
                    // Arrange
                    ReceivingProcessingMode pmode = CreateValidPMode();
                    RetryReliability r = getReliability(pmode);
                    r.IsEnabled = isEnabled;
                    r.RetryCount = retryCount;
                    r.RetryInterval = retryIntervalText;

                    // Act
                    ValidationResult result = ReceivingProcessingModeValidator.Instance.Validate(pmode);

                    // Assert
                    bool correctConfigured =
                        retryCount != default(int)
                        && TimeSpan.TryParse(retryIntervalText, out TimeSpan _)
                        && r.RetryInterval != default(TimeSpan).ToString();

                    bool expected = 
                        !isEnabled && !correctConfigured
                        || !isEnabled
                        || correctConfigured;

                    return expected.Equals(result.IsValid)
                        .Label(result.AppendValidationErrorsToErrorMessage(String.Empty))
                        .Classify(correctConfigured, "Reliability correctly configured")
                        .Classify(isEnabled, "Reliability is enabled");
                });
        }

        [Fact]
        public void PModeIsNotValid_IfNoReceiptHandlingIsPresent()
        {
            TestReceivePModeValidationFailure(pmode => pmode.ReplyHandling.ReceiptHandling = null);
        }

        [Fact]
        public void PModeIsNotValid_IfPModeIdIsMissing()
        {
            TestReceivePModeValidationFailure(pmode => pmode.Id = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoErrorHandlingIsPresent()
        {
            TestReceivePModeValidationFailure(pmode => pmode.ReplyHandling.ErrorHandling = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoReceiptHandlingSendingPModeIsPresent()
        {
            TestReceivePModeValidationFailure(pmode => pmode.ReplyHandling.SendingPMode = null);
        }

        [Fact]
        public void PModeIsNotValid_WhenReceiptHandlingSendingPModeIdIsEmpty()
        {
            TestReceivePModeValidationFailure(pmode => pmode.ReplyHandling.SendingPMode = string.Empty);
        }

        [Fact]
        public void PModeIsNotValid_IfNoReplyHandlingIsPresent()
        {
            TestReceivePModeValidationFailure(pmode => pmode.ReplyHandling = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoDeliverMethodIsPresentWhenDeliverIsEnabled()
        {
            TestReceivePModeValidationFailure(
                pmode =>
                {
                    pmode.MessageHandling.DeliverInformation.IsEnabled = true;
                    pmode.MessageHandling.DeliverInformation.DeliverMethod = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfNoPayloadReferenceMethodIsPresentWhenDeliverIsEnabled()
        {
            TestReceivePModeValidationFailure(
                pmode =>
                {
                    pmode.MessageHandling.DeliverInformation.IsEnabled = true;
                    pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfNoDeliverParametersArePresentWhenDeliverIsEnabled()
        {
            TestReceivePModeValidationFailure(
                pmode =>
                {
                    pmode.MessageHandling.DeliverInformation.IsEnabled = true;
                    pmode.MessageHandling.DeliverInformation.DeliverMethod.Parameters = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfNoPayloadReferenceParametersArePresentWhenDeliverIsEnabled()
        {
            TestReceivePModeValidationFailure(
                pmode =>
                {
                    pmode.MessageHandling.DeliverInformation.IsEnabled = true;
                    pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod.Parameters = null;
                });
        }

        [Fact]
        public void PModeIsNotValid_IfInvalidParametersArePresentInDeliverMethodWhenDeliverIsEnabled()
        {
            TestReceivePModeValidationFailure(
                pmode =>
                {
                    pmode.MessageHandling.DeliverInformation.IsEnabled = true;
                    pmode.MessageHandling.DeliverInformation
                         .DeliverMethod.Parameters.Add(new Parameter { Name = null, Value = null });
                });
        }

        [Fact]
        public void PModeIsNotValid_IfInvalidParametersArePresentInPayloadReferenceMetodWhenDeliverIsEnabled()
        {
            TestReceivePModeValidationFailure(
                pmode =>
                {
                    pmode.MessageHandling.DeliverInformation.IsEnabled = true;
                    pmode.MessageHandling.DeliverInformation
                         .PayloadReferenceMethod.Parameters.Add(new Parameter { Name = null, Value = null });
                });
        }

        [Fact]
        public void PModeIsNotValid_WhenNoMessageHandlingIsPresent()
        {
            TestReceivePModeValidationFailure(
                pmode => { pmode.MessageHandling = null; });
        }

        [Fact]
        public void PModeIsNotValid_WhenMessageHandlingIsEmpty()
        {
            TestReceivePModeValidationFailure(pmode => { pmode.MessageHandling.Item = null; });
        }

        [Fact]
        public void PModeIsNotValid_WhenMessageHandlingContainsUnknownItem()
        {
            TestReceivePModeValidationFailure(pmode => { pmode.MessageHandling.Item = new object(); });
        }

        private static void TestReceivePModeValidationSuccess(Action<ReceivingProcessingMode> f)
        {
            TestReceivePModeValidation(f, expected: true);
        }

        private static void TestReceivePModeValidationFailure(Action<ReceivingProcessingMode> f)
        {
            TestReceivePModeValidation(f, expected: false);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void TestReceivePModeValidation(Action<ReceivingProcessingMode> arrangePMode, bool expected)
        {
            // Arrange
            ReceivingProcessingMode pmode = CreateValidPMode();
            arrangePMode(pmode);

            var sut = ReceivingProcessingModeValidator.Instance;

            // Act
            ValidationResult result = sut.Validate(pmode);

            // Assert
            Assert.Equal(expected, result.IsValid);
        }

        private static ReceivingProcessingMode CreateValidPMode()
        {
            var method = new Method
            {
                Type = "deliver-type",
                Parameters = new List<Parameter> { new Parameter { Name = "parameter-name", Value = "parameter-value" } }
            };

            return new ReceivingProcessingMode
            {
                Id = "pmode-id",
                ReplyHandling = new ReplyHandlingSetting { SendingPMode = "send-pmode" },
                MessageHandling =
                {
                    DeliverInformation =
                    {
                        IsEnabled = true,
                        DeliverMethod = method,
                        PayloadReferenceMethod = method
                    }
                }
            };
        }
    }
}
