using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Eu.EDelivery.AS4.Validators
{
    public static class ValidationRuleExtensions
    {
        public static IRuleBuilderOptions<T, TProps> ForEach<T, TProps, TProp>(
            this IRuleBuilderOptions<T, TProps> rule, 
            Func<TProp, bool> predicate) where TProps : IEnumerable<TProp>
        {
            return rule.Must(ps => ps?.All(predicate) ?? false);
        }
    }
}
