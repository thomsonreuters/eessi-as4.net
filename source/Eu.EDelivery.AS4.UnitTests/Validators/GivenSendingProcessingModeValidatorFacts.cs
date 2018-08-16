using System;
using System.Linq;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using FsCheck;
using FsCheck.Xunit;
using Org.BouncyCastle.Asn1.Cms;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Validators.ValidationInputGenerators;
using static Eu.EDelivery.AS4.UnitTests.Validators.ValidationOutputAssertions;

namespace Eu.EDelivery.AS4.UnitTests.Validators
{
    public class GivenSendingProcessingModeValidatorFacts
    {
        [Property]
        public Property Encryption_Certificate_Should_Be_Specified_When_Encryption_Is_Enabled_For_A_Non_DynamicDiscovery_Setup(
            bool isEnabled,
            string findValue,
            string certificate,
            string smpProfile)
        {
            var genDynamicDiscoveryProfile = Gen.OneOf(
                Gen.Constant((DynamicDiscoveryConfiguration) null),
                Gen.Fresh(() => new DynamicDiscoveryConfiguration { SmpProfile = smpProfile }));

            return Prop.ForAll(
                CreateEncryptionCertificateInfoGen(findValue, certificate).ToArbitrary(),
                genDynamicDiscoveryProfile.ToArbitrary(),
                (cert, dynamicDiscovery) =>
                {
                    // Arrange
                    var pmode = ValidSendingPModeFactory.Create();
                    pmode.DynamicDiscovery = dynamicDiscovery;
                    pmode.Security.Encryption = new Encryption
                    {
                        IsEnabled = isEnabled,
                        EncryptionCertificateInformation = cert.Item2,
                        CertificateType = cert.Item1
                    };

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool specifiedDynamicDiscoveryProfile =
                        !String.IsNullOrWhiteSpace(dynamicDiscovery?.SmpProfile);

                    bool specifiedCertFindCriteria =
                        cert.Item1 == PublicKeyCertificateChoiceType.CertificateFindCriteria
                        && cert.Item2 is CertificateFindCriteria c
                        && !String.IsNullOrWhiteSpace(c.CertificateFindValue);

                    bool specifiedPublicKeyCert =
                        cert.Item1 == PublicKeyCertificateChoiceType.PublicKeyCertificate
                        && cert.Item2 is PublicKeyCertificate k
                        && !String.IsNullOrWhiteSpace(k.Certificate);

                    bool specifiedEncryptionCert = specifiedCertFindCriteria || specifiedPublicKeyCert;
                    return result.IsValid
                        .Equals(specifiedEncryptionCert && isEnabled && !specifiedDynamicDiscoveryProfile)
                        .Or(!isEnabled)
                        .Or(specifiedDynamicDiscoveryProfile)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but encryption certificate {(specifiedEncryptionCert ? "is" : "isn't")} specified " +
                            $"with a {cert.Item1} while the encryption is {(isEnabled ? "enabled" : "disabled")} " +
                            $"for a {(specifiedDynamicDiscoveryProfile ? "configured" : "non-configured")} Dynamic Discovery");
                });
        }

        private static Gen<Tuple<PublicKeyCertificateChoiceType, object>> CreateEncryptionCertificateInfoGen(
            string findValue, 
            string certificate)
        {
            var genCertFindCriteria = Gen.OneOf(
                Gen.Constant(Tuple.Create(
                    PublicKeyCertificateChoiceType.CertificateFindCriteria,
                    (object) null)),
                Gen.Fresh(() => Tuple.Create(
                    PublicKeyCertificateChoiceType.CertificateFindCriteria,
                    (object) new CertificateFindCriteria { CertificateFindValue = findValue })));

            var genPublicKeyCert = Gen.OneOf(
                Gen.Constant(Tuple.Create(
                    PublicKeyCertificateChoiceType.PublicKeyCertificate,
                    (object) null)),
                Gen.Fresh(() => Tuple.Create(
                    PublicKeyCertificateChoiceType.PublicKeyCertificate,
                    (object) new PublicKeyCertificate { Certificate = certificate })));

            return Gen.OneOf(genCertFindCriteria, genPublicKeyCert);
        }

        [Property]
        public Property Signing_Certificate_Should_Be_Specified_When_Signing_Is_Enabled(
            bool isEnabled,
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
                    SendingProcessingMode pmode = ValidSendingPModeFactory.Create();
                    pmode.Security.Signing = new Signing
                    {
                        IsEnabled = isEnabled,
                        SigningCertificateInformation = cert.Item2,
                        CertificateType = cert.Item1
                    };

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool specifiedCertFindCriteria =
                        cert.Item1 == PrivateKeyCertificateChoiceType.CertificateFindCriteria
                        && cert.Item2 is CertificateFindCriteria c
                        && !String.IsNullOrWhiteSpace(c.CertificateFindValue);

                    bool specifiedPrivateKeyCert =
                        cert.Item1 == PrivateKeyCertificateChoiceType.PrivateKeyCertificate
                        && cert.Item2 is PrivateKeyCertificate k
                        && !String.IsNullOrWhiteSpace(k.Certificate)
                        && !String.IsNullOrWhiteSpace(k.Password);

                    bool specifiedCertInfo = specifiedCertFindCriteria || specifiedPrivateKeyCert;
                    return result.IsValid
                        .Equals(specifiedCertInfo && isEnabled)
                        .Or(!isEnabled)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but signing certificate {(specifiedCertInfo ? "is" : "isn't")} specified " +
                            $"with a {cert.Item1} while the signing is {(isEnabled ? "enabled" : "disabled")}");
                });
        }

        [Property]
        public Property Tls_Certificate_Should_Be_Specified_When_Tls_Is_Enabled(
            bool isEnabled, 
            string clientCertFindValue,
            string password,
            string certificate)
        {
            return Prop.ForAll(
                CreateTlsCertificateInfoGen(clientCertFindValue, password, certificate)
                    .ToArbitrary(),
                tls =>
                {
                    // Arrange
                    SendingProcessingMode pmode = ValidSendingPModeFactory.Create();
                    pmode.PushConfiguration.TlsConfiguration = new TlsConfiguration
                    {
                        IsEnabled = isEnabled,
                        ClientCertificateInformation = tls.Item2,
                        CertificateType = tls.Item1
                    };

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool specifiedClientCertRef =
                        tls.Item1 == TlsCertificateChoiceType.ClientCertificateReference
                        && tls.Item2 is ClientCertificateReference clientCertRef
                        && !String.IsNullOrWhiteSpace(clientCertRef.ClientCertificateFindValue);

                    bool specifiedPrivateKeyCert =
                        tls.Item1 == TlsCertificateChoiceType.PrivateKeyCertificate
                        && tls.Item2 is PrivateKeyCertificate privateKeyCert
                        && !String.IsNullOrWhiteSpace(privateKeyCert.Certificate)
                        && !String.IsNullOrWhiteSpace(privateKeyCert.Password);

                    bool specifiedCert = specifiedClientCertRef || specifiedPrivateKeyCert;
                    return result.IsValid
                        .Equals(specifiedCert && isEnabled)
                        .Or(!isEnabled)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but TLS client certificate {(specifiedCert ? "is" : "isn't")} specified " +
                            $"with a {tls.Item1} while the TLS configuration is {(isEnabled ? "enabled" : "disabled")}");
                });
        }

        private static Gen<Tuple<TlsCertificateChoiceType, object>> CreateTlsCertificateInfoGen(
            string clientCertFindValue, 
            string password, 
            string certificate)
        {
            var genClientCertRef = Gen.OneOf(
                Gen.Constant(Tuple.Create(
                    TlsCertificateChoiceType.ClientCertificateReference, 
                    (object) null)),
                Gen.Fresh(() => Tuple.Create(
                    TlsCertificateChoiceType.ClientCertificateReference,
                    (object) new ClientCertificateReference { ClientCertificateFindValue = clientCertFindValue })));

            var genPrivateKeyCert = Gen.OneOf(
                Gen.Constant(Tuple.Create(
                    TlsCertificateChoiceType.PrivateKeyCertificate, 
                    (object) null)),
                Gen.Fresh(() => Tuple.Create(
                    TlsCertificateChoiceType.PrivateKeyCertificate,
                    (object) new PrivateKeyCertificate { Password = password, Certificate = certificate })));

            return Gen.OneOf(genClientCertRef, genPrivateKeyCert);
        }

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
                DynamicDiscovery = new DynamicDiscoveryConfiguration { SmpProfile = null },
                PushConfiguration = new PushConfiguration { Protocol = { Url = url } }
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
                    Tuple.Create(1, Arb.From<string>().Generator),
                    Tuple.Create(2, Gen.Constant(retryInterval.ToString())))
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

        [Property]
        public Property RetryCount_And_RetryInterval_Should_Be_Specified_When_ReceptionAwarness_Is_Enabled(
            bool isEnabled,
            int retryCount,
            TimeSpan retryInterval)
        {
            return Prop.ForAll(
                Gen.Frequency(
                    Tuple.Create(1, Arb.Generate<string>()),
                    Tuple.Create(2, Gen.Constant(retryInterval.ToString())))
                   .ToArbitrary(),
                retryIntervalText =>
                {
                    // Arrange
                    SendingProcessingMode pmode = ValidSendingPModeFactory.Create();
                    var r = new ReceptionAwareness
                    {
                        IsEnabled = isEnabled,
                        RetryCount = retryCount,
                        RetryInterval = retryIntervalText
                    };
                    pmode.Reliability.ReceptionAwareness = r;

                    // Act
                    ValidationResult result = ExerciseValidation(pmode);

                    // Assert
                    bool validRetryCount = r.RetryCount > 0;
                    bool validRetryInterval = r.RetryInterval.AsTimeSpan() > default(TimeSpan);
                    return result.IsValid
                        .Equals(isEnabled 
                                && validRetryCount 
                                && validRetryInterval)
                        .Or(!isEnabled)
                        .Label(
                            $"Validation has {(result.IsValid ? "succeeded" : "failed")} " +
                            $"but the RetryCount {(validRetryCount ? ">" : "<=")} 0 (was {r.RetryCount}) and " +
                            $"RetryInterval {(validRetryInterval ? ">" : "<=")} {default(TimeSpan)} (was {r.RetryInterval}) " +
                            $"while the ReceptionAwareness is {(isEnabled ? "enabled" : "disabled")}");
                });
        }

        [Property]
        public Property NotifyMethod_Shoud_Be_Specified_For_ReceiptHandling_When_We_Must_Notify_MessageProducer(
            bool notifyMessageProducer)
        {
            return NotifyMethod_Should_Be_Specified_When_We_Notify_MessageProducer(
                notifyMessageProducer,
                pmode => pmode.ReceiptHandling);
        }

        [Property]
        public Property NotifyMethod_Should_Be_Specified_For_ErrorHandling_When_We_Must_Notify_MessageProducer(
            bool notifyMessageProducer)
        {
            return NotifyMethod_Should_Be_Specified_When_We_Notify_MessageProducer(
                notifyMessageProducer,
                pmode => pmode.ErrorHandling);
        }

        [Property]
        public Property NotifyMethod_Should_Be_Specified_For_ExceptionHandling_When_We_Must_Notify_MessageProducer(
            bool notifyMessageProducer)
        {
            return NotifyMethod_Should_Be_Specified_When_We_Notify_MessageProducer(
                notifyMessageProducer,
                pmode => pmode.ExceptionHandling);
        }

        private static Property NotifyMethod_Should_Be_Specified_When_We_Notify_MessageProducer(
            bool notifyMessageProducer,
            Func<SendingProcessingMode, SendHandling> getHandling)
        {
            return Prop.ForAll(
                CreateMethodGen().ToArbitrary(),
                method =>
                {
                    // Arrange
                    var pmode = ValidSendingPModeFactory.Create();
                    SendHandling sendHandling = getHandling(pmode);
                    sendHandling.NotifyMessageProducer = notifyMessageProducer;
                    sendHandling.NotifyMethod = method;

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

        private static ValidationResult ExerciseValidation(SendingProcessingMode pmode)
        {
            return SendingProcessingModeValidator.Instance.Validate(pmode);
        }
    }
}