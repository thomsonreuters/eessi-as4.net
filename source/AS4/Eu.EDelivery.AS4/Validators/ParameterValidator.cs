using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator to check the <see cref="Parameter"/> Model
    /// </summary>
    internal class ParameterValidator : AbstractValidator<Parameter>
    {
        public ParameterValidator()
        {
            RuleFor(param => param.Name).NotNull();
            RuleFor(param => param.Value).NotNull();
        }
    }
}
