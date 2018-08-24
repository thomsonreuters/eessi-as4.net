using Eu.EDelivery.AS4.Model.PMode;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validator to check the <see cref="Parameter"/> Model
    /// </summary>
    internal class ParameterValidator : AbstractValidator<Parameter>
    {
        public static ParameterValidator Instance = new ParameterValidator();

        public ParameterValidator()
        {
            RuleFor(param => param.Name).NotEmpty();
            RuleFor(param => param.Value).NotEmpty();
        }
    }
}
