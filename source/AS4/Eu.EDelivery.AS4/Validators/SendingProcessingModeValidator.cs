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
        public SendingProcessingModeValidator()
        {
            RuleFor(pmode => pmode.Id).NotEmpty();

            RulesForPullConfiguration();
            RulesForPushConfiguration();
            RulesForReceiptHandling();
            RulesForErrorHandling();
            RulesForExceptionHandling();
            RulesForSigning();
            RulesForEncryption();
        }

        private void RulesForPullConfiguration()
        {
            Func<SendingProcessingMode, bool> isPulling =
                pmode => pmode.MepBinding == MessageExchangePatternBinding.Pull;

            RuleFor(pmode => pmode.PullConfiguration.Protocol).NotNull().When(isPulling);
            RuleFor(pmode => pmode.PullConfiguration.Protocol.Url).NotEmpty().When(isPulling);
        }

        private void RulesForPushConfiguration()
        {
            Func<SendingProcessingMode, bool> isPushing =
                pmode => pmode.MepBinding == MessageExchangePatternBinding.Push;

            RuleFor(pmode => pmode.PushConfiguration.Protocol).NotNull().When(isPushing);
            RuleFor(pmode => pmode.PushConfiguration.Protocol.Url).NotEmpty().When(isPushing);
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

            RuleFor(pmode => pmode.Security.Signing.PrivateKeyFindValue).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => pmode.Security.Signing.Algorithm).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => pmode.Security.Signing.HashFunction).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => Constants.Algoritms.Contains(pmode.Security.Signing.Algorithm))
                .NotNull()
                .When(isSigningEnabled);
            RuleFor(pmode => Constants.HashFunctions.Contains(pmode.Security.Signing.HashFunction))
                .NotNull()
                .When(isSigningEnabled);
        }

        private void RulesForEncryption()
        {
            Func<SendingProcessingMode, bool> isEncryptionEnabled = pmode => pmode.Security.Encryption.IsEnabled;
            RuleFor(pmode => pmode.Security.Encryption.PublicKeyFindValue).NotNull().When(isEncryptionEnabled);
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

            var keysizes = new[] {128, 192, 256};
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