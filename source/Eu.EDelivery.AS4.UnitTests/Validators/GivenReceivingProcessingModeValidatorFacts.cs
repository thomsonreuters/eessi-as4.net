using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Validators.ValidationInputGenerators;
using static Eu.EDelivery.AS4.UnitTests.Validators.ValidationOutputAssertions;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    public class GivenReceivingProcessingModeValidatorFacts
    {
        [Fact]
        public void Start_Receiving_PMode_Is_Valid()
        {
            Assert.True(ExerciseValidation(CreateValidPMode()).IsValid);
        }

        [Property]
        public Property Decryption_Certificate_Should_Be_Specified_When_Decryption_Is_Allowed_Or_Required(
            Limit encryption,
            string findValue,
            string certificate,
            string password)
        {
            return Prop.ForAll(
                CreatePrivateKeyCertificateGen(findValue, certificate, password)
                    .ToArbitrary(),
                cert =>
                {
                    // Arrange
                    ReceivingProcessingMode pmode = CreateValidPMode();
                    pmode.Security.Decryption = new Decryption
                    {
                        Encryption = encryption,
                        DecryptCertificateInformation = cert.Item2,
                        CertificateType = cert.Item1
                    };

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool allowedOrRequired = 
                        encryption == Limit.Allowed 
                        || encryption == Limit.Required;

                    bool specifiedCertFindCriteria =
                        cert.Item1 == PrivateKeyCertificateChoiceType.CertificateFindCriteria
                        && cert.Item2 is CertificateFindCriteria c
                        && !String.IsNullOrWhiteSpace(c.CertificateFindValue);

                    bool specifiedPrivateKeyCert =
                        cert.Item1 == PrivateKeyCertificateChoiceType.PrivateKeyCertificate
                        && cert.Item2 is PrivateKeyCertificate k
                        && !String.IsNullOrWhiteSpace(k.Certificate)
                        && !String.IsNullOrWhiteSpace(k.Password);

                    bool specifiedDecryptionCert = specifiedCertFindCriteria || specifiedPrivateKeyCert;
                    return result.IsValid
                        .Equals(specifiedDecryptionCert && allowedOrRequired)
                        .Or(!allowedOrRequired)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but decryption certificate {(specifiedDecryptionCert ? "is" : "isn't")} specified " +
                            $"with a {cert.Item1} while the encryption limit is {encryption}");
                });
        }

        [Property]
        public Property ReplyHandling_Must_Be_Specified_When_There_Isnt_A_Forward_Element(
            string responsePMode,
            string forwardPMode)
        {
            var genForward = Gen.OneOf(
                Gen.Constant((object) null),
                Gen.Fresh(() => (object) new Forward { SendingPMode = forwardPMode }),
                Gen.Fresh(() => (object) new Deliver()));

            var genReplyHandling = Gen.OneOf(
                Gen.Constant((ReplyHandlingSetting) null),
                Gen.Fresh(() => new ReplyHandlingSetting { SendingPMode = responsePMode }));

            return Prop.ForAll(
                genForward.ToArbitrary(),
                genReplyHandling.ToArbitrary(),
                (messageHandlingImp, replyHandling) =>
                {
                    // Arrange
                    ReceivingProcessingMode pmode = CreateValidPMode();
                    pmode.ReplyHandling = replyHandling;
                    pmode.MessageHandling.Item = messageHandlingImp;

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool specifiedDeliver = messageHandlingImp is Deliver;
                    bool specifiedForward =
                        messageHandlingImp is Forward f
                        && !String.IsNullOrWhiteSpace(f.SendingPMode);

                    bool specifiedReplyHandling = 
                        !String.IsNullOrWhiteSpace(replyHandling?.SendingPMode);

                    return result.IsValid
                        .Equals(specifiedReplyHandling && specifiedDeliver)
                        .Or(!specifiedReplyHandling && specifiedForward)
                        .Or(specifiedReplyHandling && specifiedForward)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but ReplyHandling {(specifiedReplyHandling ? "is" : "isn't")} specified and " +
                            $"MessageHandling is {(specifiedDeliver ? "a Deliver" : specifiedForward ? "a Forward" : "empty")} element ");
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

        [Fact]
        public void PModeIsNotValid_IfNoReceiptHandlingIsPresent()
        {
            TestReceivePModeValidationFailure(pmode => pmode.ReplyHandling.ReceiptHandling = null);
        }

        [Fact]
        public void PModeIsNotValid_IfNoErrorHandlingIsPresent()
        {
            TestReceivePModeValidationFailure(pmode => pmode.ReplyHandling.ErrorHandling = null);
        }

        [Property]
        public Property DeliverMethods_Requires_Either_Empty_Or_Filled_Name_And_Value_Attributes_When_Delivery_Is_Enabled(
            bool isEnabled)
        {
            return Prop.ForAll(
                CreateMethodGen().ToArbitrary(),
                CreateMethodGen().ToArbitrary(),
                (deliver, payloadRef) =>
                {
                    // Arrange
                    ReceivingProcessingMode pmode = CreateValidPMode();
                    pmode.MessageHandling.Item = null;
                    pmode.MessageHandling.Item = new Deliver
                    {
                        IsEnabled = isEnabled,
                        DeliverMethod = deliver,
                        PayloadReferenceMethod = payloadRef
                    };

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert

                    bool specifiedDeliver = SpecifiedMethod(deliver);
                    bool specifiedPayloadRef = SpecifiedMethod(payloadRef);
                    return result.IsValid
                        .Equals(isEnabled && specifiedDeliver && specifiedPayloadRef)
                        .Or(!isEnabled)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but the DeliverMethod {(specifiedDeliver ? "is" : "isn't")} specified " +
                            $"and the PayloadReferenceMethod {(specifiedPayloadRef ? "is" : "isn't")} specified " +
                            $"while the Delivery is {(isEnabled ? "enabled" : "disabled")}");
                });
        }

        [Property]
        public Property ExceptionHandling_Requires_To_Have_Specified_Method_When_The_MessageProducer_Must_Be_Notified(
            bool notifyMessageProducer)
        {
            return Prop.ForAll(
                CreateMethodGen().ToArbitrary(),
                method =>
                {
                    // Arrange
                    ReceivingProcessingMode pmode = CreateValidPMode();
                    pmode.ExceptionHandling = new ReceiveHandling
                    {
                        NotifyMessageConsumer = notifyMessageProducer,
                        NotifyMethod = method
                    };

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool specifiedNotifyMethod = SpecifiedMethod(method);
                    return result.IsValid
                        .Equals(notifyMessageProducer && specifiedNotifyMethod)
                        .Or(!notifyMessageProducer)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but the NotifyMethod {(specifiedNotifyMethod ? "is" : "isn't")} specified " +
                            $"while the NotifyMessageProducer is {(notifyMessageProducer ? "enabled" : "disabled")}");
                });
        }

        [Fact]
        public void PModeIsNotValid_WhenNoMessageHandlingIsPresent()
        {
            TestReceivePModeValidationFailure(pmode => pmode.MessageHandling = null);
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

            // Act
            ValidationResult result = ExerciseValidation(pmode);

            // Assert
            Assert.Equal(expected, result.IsValid);
        }

        private static ValidationResult ExerciseValidation(ReceivingProcessingMode fixture)
        {
            return ReceivingProcessingModeValidator.Instance.Validate(fixture);
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
