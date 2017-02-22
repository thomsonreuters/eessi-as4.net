using System;
using Castle.Core.Internal;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator responsible for Validation Model <see cref="SendingProcessingMode" />
    /// </summary>
    public class SendingProcessingModeValidator : AbstractValidator<SendingProcessingMode>, IValidator<SendingProcessingMode>
    {
        private readonly ILogger _logger;

        public SendingProcessingModeValidator()
        {
            this._logger = LogManager.GetCurrentClassLogger();

            RuleFor(pmode => pmode.Id).NotEmpty();

            RulesForPushConfiguration();
            RulesForReceiptHandling();
            RulesForErrorHandling();
            RulesForExceptionHandling();
            RulesForSigning();
            RulesForEncryption();
        }

        private void RulesForPushConfiguration()
        {
            RuleFor(pmode => pmode.PushConfiguration.Protocol).NotNull();
            RuleFor(pmode => pmode.PushConfiguration.Protocol.Url).NotEmpty();
        }

        private void RulesForReceiptHandling()
        {
            Func<SendingProcessingMode, bool> isReceiptHandlingEnabled = pmode => pmode.ReceiptHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod).NotNull().When(isReceiptHandlingEnabled);
            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod.Parameters).NotNull().SetCollectionValidator(new ParameterValidator()).When(isReceiptHandlingEnabled);
            RuleFor(pmode => pmode.ReceiptHandling.NotifyMethod.Type).NotNull().When(isReceiptHandlingEnabled);
        }

        private void RulesForErrorHandling()
        {
            Func<SendingProcessingMode, bool> isErrorHandlingEnabled = pmode => pmode.ErrorHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod).NotNull().When(isErrorHandlingEnabled);
            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod.Parameters).NotNull().SetCollectionValidator(new ParameterValidator()).When(isErrorHandlingEnabled);
            RuleFor(pmode => pmode.ErrorHandling.NotifyMethod.Type).NotNull().When(isErrorHandlingEnabled);
        }

        private void RulesForExceptionHandling()
        {
            Func<SendingProcessingMode, bool> isExceptionHandlingEnabled = pmode => pmode.ExceptionHandling.NotifyMessageProducer;

            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod).NotNull().When(isExceptionHandlingEnabled);
            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Parameters).NotNull().SetCollectionValidator(new ParameterValidator()).When(isExceptionHandlingEnabled);
            RuleFor(pmode => pmode.ExceptionHandling.NotifyMethod.Type).NotNull().When(isExceptionHandlingEnabled);
        }

        private void RulesForSigning()
        {
            Func<SendingProcessingMode, bool> isSigningEnabled = pmode => pmode.Security.Signing.IsEnabled;

            RuleFor(pmode => pmode.Security.Signing.PrivateKeyFindValue).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => pmode.Security.Signing.Algorithm).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => pmode.Security.Signing.HashFunction).NotEmpty().When(isSigningEnabled);
            RuleFor(pmode => Constants.Algoritms.Contains(pmode.Security.Signing.Algorithm)).NotNull().When(isSigningEnabled);
            RuleFor(pmode => Constants.HashFunctions.Contains(pmode.Security.Signing.HashFunction)).NotNull().When(isSigningEnabled);
        }

        private void RulesForEncryption()
        {
            Func<SendingProcessingMode, bool> isEncryptionEnabled = pmode => pmode.Security.Encryption.IsEnabled;
            RuleFor(pmode => pmode.Security.Encryption.PublicKeyFindValue).NotNull().When(isEncryptionEnabled);
        }


        /// <summary>
        /// Validate the given <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool IValidator<SendingProcessingMode>.Validate(SendingProcessingMode model)
        {
            ValidationResult validationResult = base.Validate(model);

            if (validationResult.IsValid) return true;
            throw ThrowHandleInvalidPModeException(model, validationResult);
        }

        private AS4Exception ThrowHandleInvalidPModeException(SendingProcessingMode pmode, ValidationResult result)
        {
            foreach (var e in result.Errors)
            {
                _logger.Error($"Sending PMode Validation Error: {e.PropertyName} = {e.ErrorMessage}");
            }

            string description = $"Sending PMode {pmode.Id} was invalid, see logging";
            this._logger.Error(description);
            
            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }
    }
}