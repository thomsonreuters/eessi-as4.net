using System;
using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator to check the <see cref="ReceivingProcessingMode" />
    /// </summary>
    public class ReceivingProcessingModeValidator : AbstractValidator<ReceivingProcessingMode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingProcessingModeValidator" /> class.
        /// </summary>
        public ReceivingProcessingModeValidator()
        {
            RuleFor(pmode => pmode.Id).NotNull();

            RulesForReplyHandling();            
            RulesForDeliver();
        }

        public static readonly ReceivingProcessingModeValidator Instance = new ReceivingProcessingModeValidator();

        private void RulesForReplyHandling()
        {
            RuleFor(pmode => pmode.ReplyHandling).NotNull().WithMessage("A ReplyHandling element must be present in the Receiving PMode.");
            When(pmode => pmode.ReplyHandling != null, delegate
            {
                RuleFor(pmode => pmode.ReplyHandling.SendingPMode).NotEmpty().WithMessage("A SendingPMode must be defined in the ReplyHandling section.");
                RuleFor(pmode => pmode.ReplyHandling.ReceiptHandling).NotNull().WithMessage("A ReceiptHandling element must be defined in the ReplyHandling section.");
                RuleFor(pmode => pmode.ReplyHandling.ErrorHandling).NotNull().WithMessage("An ErrorHandling element must be defined in the ReplyHandling section.");
            });
        }

        private void RulesForDeliver()
        {
            Func<ReceivingProcessingMode, bool> isDeliverEnabled = pmode => pmode.Deliver?.IsEnabled == true;

            RuleFor(pmode => pmode.Deliver.DeliverMethod).NotNull().When(isDeliverEnabled);
            RuleFor(pmode => pmode.Deliver.PayloadReferenceMethod).NotNull().When(isDeliverEnabled);

            Func<ReceivingProcessingMode, bool> isDeliverMethodPresent = pmode => pmode.Deliver.DeliverMethod != null;
            RuleFor(pmode => pmode.Deliver.DeliverMethod.Type)
                .NotNull()
                .When(pmode => isDeliverMethodPresent(pmode) && isDeliverEnabled(pmode));
            RuleFor(pmode => pmode.Deliver.DeliverMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(pmode => isDeliverMethodPresent(pmode) && isDeliverEnabled(pmode));

            Func<ReceivingProcessingMode, bool> isPayloadReferencePresent =
                pmode => pmode.Deliver.PayloadReferenceMethod != null;
            RuleFor(pmode => pmode.Deliver.PayloadReferenceMethod.Type)
                .NotNull()
                .When(pmode => isPayloadReferencePresent(pmode) && isDeliverEnabled(pmode));
            RuleFor(pmode => pmode.Deliver.PayloadReferenceMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(pmode => isPayloadReferencePresent(pmode) && isDeliverEnabled(pmode));
        }
    }
}