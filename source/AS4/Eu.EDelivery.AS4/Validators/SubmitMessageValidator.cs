using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Submit;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validate the <see cref="SubmitMessage"/> Model
    /// </summary>
    public class SubmitMessageValidator : AbstractValidator<SubmitMessage>
    {
        public SubmitMessageValidator()
        {
            RuleFor(s => s.Collaboration).NotNull();
            RuleFor(s => s.Collaboration.AgreementRef).NotNull();
            RuleFor(s => s.Collaboration.AgreementRef.PModeId).NotEmpty();

            RuleFor(s => s.Payloads).SetCollectionValidator(new PayloadValidator());
        }
    }

    internal class PayloadValidator : AbstractValidator<Payload>
    {
        public PayloadValidator()
        {
            RuleFor(p => p.Location).NotEmpty();
        }
    }
}