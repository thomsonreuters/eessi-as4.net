using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator responsible for Validation Model <see cref="SendingProcessingMode" />
    /// </summary>
    public class SendingProcessingModeValidator : AbstractValidator<SendingProcessingMode>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="SendingProcessingModeValidator"/> class.
        /// </summary>
        private SendingProcessingModeValidator()
        {
            RuleFor(pmode => pmode.Id).NotEmpty();

            RulesForDynamicDiscoveryConfiguration();
            RulesForPullConfiguration();
            RulesForReceiptHandling();
            RulesForErrorHandling();
            RulesForExceptionHandling();
            RulesForSigning();
            RulesForEncryption();
        }

        public static readonly SendingProcessingModeValidator Instance = new SendingProcessingModeValidator();

        private void RulesForDynamicDiscoveryConfiguration()
        {
            Func<SendingProcessingMode, bool> isPushing = pmode => pmode.MepBinding == MessageExchangePatternBinding.Push;

            When(p => isPushing(p), delegate
            {
                RuleFor(pmode => pmode.DynamicDiscovery).NotNull()
                                                        .When(pmode => pmode.PushConfigurationSpecified == false)
                                                        .WithMessage("The DynamicDiscovery or PushConfiguration element must be specified.");
            });
        }

        private void RulesForPullConfiguration()
        {
            Func<SendingProcessingMode, bool> isPulling =
                pmode => pmode.MepBinding == MessageExchangePatternBinding.Pull;
            When(p => isPulling(p), delegate
            {
                RuleFor(pmode => pmode.PushConfiguration).Null().WithMessage("PushConfiguration element should not be specified when MEP = Pull");
            });
        }

        private void RulesForReceiptHandling()
        {
            Func<SendingProcessingMode, bool> isReceiptHandlingEnabled =
                pmode => pmode.ReceiptHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod).NotNull().When(isReceiptHandlingEnabled);
            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(isReceiptHandlingEnabled);
            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod.Type).NotNull().When(isReceiptHandlingEnabled);
        }

        private void RulesForErrorHandling()
        {
            Func<SendingProcessingMode, bool> isErrorHandlingEnabled =
                pmode => pmode.ErrorHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod).NotNull().When(isErrorHandlingEnabled);
            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(isErrorHandlingEnabled);
            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod.Type).NotNull().When(isErrorHandlingEnabled);
        }

        private void RulesForExceptionHandling()
        {
            Func<SendingProcessingMode, bool> isExceptionHandlingEnabled =
                pmode => pmode.ExceptionHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod).NotNull().When(isExceptionHandlingEnabled);
            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(isExceptionHandlingEnabled);
            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Type).NotNull().When(isExceptionHandlingEnabled);
        }

        private void RulesForSigning()
        {
            Func<SendingProcessingMode, bool> isSigningEnabled = pmode => pmode.Security.Signing.IsEnabled;

            When(isSigningEnabled, delegate
            {
                RuleFor(pmode => pmode.Security.Signing.SigningCertificateInformation).NotNull()
                                                                               .WithMessage(
                                                                                   "Signing certificate information must be specified when signing is enabled.");
            });

            RuleFor(pmode => pmode.Security.Signing.Algorithm).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => pmode.Security.Signing.HashFunction).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => Constants.SignAlgorithms.IsSupported(pmode.Security.Signing.Algorithm))
                .NotNull()
                .When(isSigningEnabled);
            RuleFor(pmode => Constants.HashFunctions.IsSupported(pmode.Security.Signing.HashFunction))
                .NotNull()
                .When(isSigningEnabled);
        }

        private void RulesForEncryption()
        {
            Func<SendingProcessingMode, bool> isEncryptionEnabled = pmode => pmode.Security.Encryption.IsEnabled && pmode.DynamicDiscoverySpecified == false;

            When(isEncryptionEnabled, delegate
            {
                RuleFor(pmode => pmode.Security.Encryption.EncryptionCertificateInformation).NotNull()
                                         .WithMessage("Encryption certificate information must be specified when encryption is enabled");
            });

        }

        /// <summary>
        /// Validates the specified instance
        /// </summary>
        /// <param name="instance">The object to validate</param>
        /// <returns>A ValidationResult object containing any validation failures</returns>
        public override ValidationResult Validate(SendingProcessingMode instance)
        {
            PreConditions(instance);

            return base.Validate(instance);
        }

        private static void PreConditions(SendingProcessingMode model)
        {
            try
            {
                ValidateKeySize(model);
            }
            catch (Exception exception)
            {
                Logger.Debug(exception);
            }
        }

        private static void ValidateKeySize(SendingProcessingMode model)
        {
            if (model.Security?.Encryption?.IsEnabled == false || model.Security?.Encryption == null)
            {
                return;
            }

            var keysizes = new[] { 128, 192, 256 };
            int actualKeySize = model.Security.Encryption.AlgorithmKeySize;

            if (!keysizes.Contains(actualKeySize) && model.Security?.Encryption != null)
            {
                int defaultKeySize = Encryption.Default.AlgorithmKeySize;
                Logger.Warn($"Invalid Encryption 'Key Size': {actualKeySize}, {defaultKeySize} is taken as default");
                model.Security.Encryption.AlgorithmKeySize = defaultKeySize;
            }
        }
    }
}