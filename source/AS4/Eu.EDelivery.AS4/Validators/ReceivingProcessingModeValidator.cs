using System;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;
using FluentValidation.Results;
using NLog;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator to check the <see cref="ReceivingProcessingMode"/>
    /// </summary>
    public class ReceivingProcessingModeValidator : AbstractValidator<ReceivingProcessingMode>, IValidator<ReceivingProcessingMode>
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingProcessingModeValidator" /> class
        /// </summary>
        public ReceivingProcessingModeValidator()
        {
            RuleFor(pmode => pmode.Id).NotNull();

            RulesForReceiptHandling();
            RulesForErrorHandling();
            RulesForDeliver();
        }

        private void RulesForReceiptHandling()
        {
            RuleFor(pmode => pmode.ReceiptHandling).NotNull();
            RuleFor(pmode => pmode.ReceiptHandling.SendingPMode).NotNull();
        }

        private void RulesForErrorHandling()
        {
            RuleFor(pmode => pmode.ErrorHandling).NotNull();
            RuleFor(pmode => pmode.ErrorHandling.SendingPMode).NotNull();
        }

        private void RulesForDeliver()
        {
            Func<ReceivingProcessingMode, bool> isDeliverEnabled = pmode => pmode.Deliver.IsEnabled;

            RuleFor(pmode => pmode.Deliver.DeliverMethod).NotNull().When(isDeliverEnabled);
            RuleFor(pmode => pmode.Deliver.PayloadReferenceMethod).NotNull().When(isDeliverEnabled);

            RuleFor(pmode => pmode.Deliver.DeliverMethod.Type).NotNull().When(isDeliverEnabled);
            RuleFor(pmode => pmode.Deliver.DeliverMethod.Parameters).NotNull().SetCollectionValidator(new ParameterValidator()).When(isDeliverEnabled);

            RuleFor(pmode => pmode.Deliver.PayloadReferenceMethod.Type).NotNull().When(isDeliverEnabled);
            RuleFor(pmode => pmode.Deliver.PayloadReferenceMethod.Parameters).NotNull().SetCollectionValidator(new ParameterValidator()).When(isDeliverEnabled);
        }

        /// <summary>
        /// Validate the given <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        void IValidator<ReceivingProcessingMode>.Validate(ReceivingProcessingMode model)
        {
            ValidationResult validationResult = base.Validate(model);

            if (!validationResult.IsValid)
            {
                throw ThrowHandleInvalidPModeException(model, validationResult);
            }
        }

        private static AS4Exception ThrowHandleInvalidPModeException(IPMode pmode, ValidationResult result)
        {
            foreach (ValidationFailure error in result.Errors)
            {
                Logger.Error($"Receiving PMode Validation Error: {error.PropertyName} = {error.ErrorMessage}");
            }

            string description = $"Receiving PMode {pmode.Id} was invalid, see logging";
            Logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }
    }
}
