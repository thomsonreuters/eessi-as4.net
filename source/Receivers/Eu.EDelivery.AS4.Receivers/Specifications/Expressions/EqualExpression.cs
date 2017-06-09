using System;
using System.Collections.Generic;
using System.Linq;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal sealed class EqualExpression<T> : IExpression
    {
        private static readonly IDictionary<string, Func<string, string, T, IExpression>> Expressions
            = new Dictionary<string, Func<string, string, T, IExpression>>
            {
                ["="] = (name, value, set) => new SameExpression<T>(name, value, set),
                ["IS"] = (name, value, set) => new SameExpression<T>(name, value, set),
                ["IS NOT"] = (name, value, set) => new NotSameExpression<T>(name, value, set),
                ["!="] = (name, value, set) => new NotSameExpression<T>(name, value, set)
            };

        private readonly IExpression _innerExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualExpression{T}" /> class.
        /// </summary>
        /// <param name="innerExpression">The inner expression.</param>
        public EqualExpression(IExpression innerExpression)
        {
            _innerExpression = innerExpression;
        }

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <returns></returns>
        public bool Evaluate()
        {
            return _innerExpression.Evaluate();
        }

        /// <summary>
        /// Fors the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="databaseSet">The database set.</param>
        /// <returns></returns>
        public static EqualExpression<T> For(string expression, T databaseSet)
        {
            string separator = expression.Contains("=")
                ? expression.Contains("!=") ? "!=" : "="
                : expression.Contains(" IS NOT ") ? "IS NOT" : "IS";

            string[] entries =
                expression.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(e => e.Trim())
                          .ToArray();

            string left = entries.ElementAtOrDefault(0);
            string right = entries.ElementAtOrDefault(1);

            ThrowIfInvalidEqualEspression(left, separator, right);

            return new EqualExpression<T>(Expressions[separator](left, right, databaseSet));
        }

        private static void ThrowIfInvalidEqualEspression(string left, string separator, string right)
        {
            if (left == null || left.Contains(" ") || right == null || right.Contains(" "))
            {
                throw new FormatException($"Equality expression is invalid: '{left}{separator}{right}'");
            }
        }
    }    
}