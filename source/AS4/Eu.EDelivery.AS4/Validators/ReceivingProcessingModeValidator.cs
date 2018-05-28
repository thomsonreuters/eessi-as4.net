using System;
using Eu.EDelivery.AS4.Extensions;
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
        private ReceivingProcessingModeValidator()
        {
            RuleFor(pmode => pmode.Id).NotNull();

            RulesForReplyHandling();
            RulesForExceptionHandling();
            RulesForMessageHandling();
            RulesForDeliver();
            RulesForForwarding();
        }

        public static readonly ReceivingProcessingModeValidator Instance = new ReceivingProcessingModeValidator();

        private void RulesForReplyHandling()
        {
            Func<ReceivingProcessingMode, bool> notForForwarding = 
                pmode => pmode.MessageHandling?.ForwardInformation == null;

            RuleFor(pmode => pmode.ReplyHandling)
                .NotNull()
                .WithMessage("A ReplyHandling element must be present in the Receiving PMode.")
                .When(notForForwarding);

            When(
                pmode => pmode.ReplyHandling != null,
                () =>
                {
                    RuleFor(pmode => pmode.ReplyHandling.SendingPMode)
                        .NotEmpty()
                        .WithMessage("A SendingPMode must be defined in the ReplyHandling section.")
                        .When(notForForwarding);

                    RuleFor(pmode => pmode.ReplyHandling.ReceiptHandling)
                        .NotNull()
                        .WithMessage("A ReceiptHandling element must be defined in the ReplyHandling section.");
                    RuleFor(pmode => pmode.ReplyHandling.ErrorHandling)
                        .NotNull()
                        .WithMessage("An ErrorHandling element must be defined in the ReplyHandling section.");
                });
        }

        private void RulesForExceptionHandling()
        {
            Func<ReceivingProcessingMode, bool> isReliabilityEnabled =
                pmode => pmode.ExceptionHandling?.Reliability?.IsEnabled == true;

            RuleFor(pmode => pmode.ExceptionHandling.Reliability.RetryCount)
                .NotEqual(default(int))
                .When(isReliabilityEnabled);

            RuleFor(pmode => pmode.ExceptionHandling.Reliability.RetryInterval.AsTimeSpan())
                .NotEqual(default(TimeSpan))
                .When(isReliabilityEnabled);
        }

        private void RulesForMessageHandling()
        {
            RuleFor(pmode => pmode.MessageHandling)
                .NotNull()
                .WithMessage("The MessageHandling element must be declared")
                .DependentRules(
                    r => r.RuleFor(pmode => pmode.MessageHandling)
                          .Must(mh => mh.DeliverInformation != null
                                      || mh.ForwardInformation != null)
                          .WithMessage("The MessageHandling element must contain a Deliver or a Forward element"));
        }

        private void RulesForForwarding()
        {
            Func<ReceivingProcessingMode, bool> isForwarding = 
                pmode => pmode.MessageHandling?.ForwardInformation != null;

            RuleFor(pmode => pmode.MessageHandling.ForwardInformation.SendingPMode)
                .NotEmpty()
                .When(isForwarding)
                .WithMessage("When Forwarding is enabled, the Forward element must contain the ID of the Sending PMode that must be used for forwarding.");
        }

        private void RulesForDeliver()
        {
            Func<ReceivingProcessingMode, bool> isDeliverEnabled = 
                pmode => pmode.MessageHandling?.DeliverInformation?.IsEnabled == true;
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.DeliverMethod)
                .NotNull()
                .When(isDeliverEnabled);
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod)
                .NotNull()
                .When(isDeliverEnabled);

            Func<ReceivingProcessingMode, bool> isDeliverMethodPresent = 
                pmode => pmode.MessageHandling?.DeliverInformation?.DeliverMethod != null;
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.DeliverMethod.Type)
                .NotNull()
                .When(pmode => isDeliverMethodPresent(pmode) && isDeliverEnabled(pmode));
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.DeliverMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(pmode => isDeliverMethodPresent(pmode) && isDeliverEnabled(pmode));

            Func<ReceivingProcessingMode, bool> isPayloadReferencePresent =
                pmode => pmode.MessageHandling?.DeliverInformation?.PayloadReferenceMethod != null;
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod.Type)
                .NotNull()
                .When(pmode => isPayloadReferencePresent(pmode) && isDeliverEnabled(pmode));
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.PayloadReferenceMethod.Parameters)
                .NotNull()
                .SetCollectionValidator(new ParameterValidator())
                .When(pmode => isPayloadReferencePresent(pmode) && isDeliverEnabled(pmode));

            Func<ReceivingProcessingMode, bool> isReliabilityEnabled = 
                pmode => pmode.MessageHandling?.DeliverInformation?.Reliability?.IsEnabled == true;
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.Reliability.RetryCount)
                .NotEqual(default(int))
                .When(isReliabilityEnabled);
            RuleFor(pmode => pmode.MessageHandling.DeliverInformation.Reliability.RetryInterval.AsTimeSpan())
                .NotEqual(default(TimeSpan))
                .When(isReliabilityEnabled);
        }
    }
}