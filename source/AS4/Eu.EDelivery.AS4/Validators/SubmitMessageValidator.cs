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
            RuleFor(p => p.Schemas).SetCollectionValidator(new SchemaValidator());
            RuleFor(p => p.PayloadProperties).SetCollectionValidator(new PayloadPropertyValidator());
        }
    }

    internal class SchemaValidator : AbstractValidator<Schema>
    {
        public SchemaValidator()
        {
            RuleFor(s => s.Location).NotEmpty();
        }
    }

    internal class PayloadPropertyValidator : AbstractValidator<PayloadProperty>
    {
        public PayloadPropertyValidator()
        {
            RuleFor(p => p.Name).NotEmpty();
        }
    }
}