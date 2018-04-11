using System.Linq;
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

            RuleFor(s => s.Payloads).Must(ps => ps.GroupBy(p => p.Id).All(p => p.Count() == 1))
                                    .When(s => s.Payloads != null);
        }
    }
}