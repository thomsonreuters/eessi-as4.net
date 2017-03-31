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
    /// Validator to check the <see cref="ReceivingProcessingMode"/>
    /// </summary>
    public class ReceivingProcessingModeValidator : AbstractValidator<ReceivingProcessingMode>, IValidator<ReceivingProcessingMode>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the type <see cref="ReceivingProcessingModeValidator"/> class
        /// </summary>
        public ReceivingProcessingModeValidator()
        {
            this._logger = LogManager.GetCurrentClassLogger();

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
                throw ThrowHandleInvalidPModeException(model, validationResult);
        }

        private AS4Exception ThrowHandleInvalidPModeException(ReceivingProcessingMode pmode, ValidationResult result)
        {
            foreach (var e in result.Errors)
            {
                _logger.Error($"Receiving PMode Validation Error: {e.PropertyName} = {e.ErrorMessage}");
            }

            string description = $"Receiving PMode {pmode.Id} was invalid, see logging";
            this._logger.Error(description);

            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithMessageIds(Guid.NewGuid().ToString())
                .Build();
        }
    }
}
