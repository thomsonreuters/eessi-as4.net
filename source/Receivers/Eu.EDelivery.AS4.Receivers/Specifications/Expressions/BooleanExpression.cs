using System;
using System.Collections.Generic;
using System.Linq;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal sealed class BooleanExpression : IExpression
    {
        private static readonly IDictionary<string, Func<bool, bool, IExpression>> LogicalExpressions =
            new Dictionary<string, Func<bool, bool, IExpression>>
            {
                ["AND"] = (left, right) => new AndExpression(left, right),
                ["OR"] = (left, right) => new OrExpression(left, right)
            };

        private readonly IExpression _innerExpression;

        private BooleanExpression(IExpression expression)
        {
            _innerExpression = expression;
        }

        /// <summary>
        /// Evaluate a given expression arguments to a boolean expression result.
        /// </summary>
        /// <returns></returns>
        public bool Evaluate()
        {
            return _innerExpression.Evaluate();
        }

        /// <summary>
        /// Create a new <see cref="BooleanExpression" />
        /// </summary>
        /// <param name="operatorValue">The operator value.</param>
        /// <param name="leftValue">The left value.</param>
        /// <param name="rightValue">The right value.</param>
        /// <returns></returns>
        public static BooleanExpression For(string operatorValue, string leftValue, string rightValue)
        {
            ThrowIfInvalidBooleanExpression(leftValue, rightValue);

            bool.TryParse(leftValue, out bool left);
            bool.TryParse(rightValue, out bool right);

            IExpression expression = LogicalExpressions[operatorValue](left, right);

            return new BooleanExpression(expression);
        }

        private static void ThrowIfInvalidBooleanExpression(string leftValue, string rightValue)
        {
            var operators = new[] {"AND", "OR"};
            if (operators.Contains(leftValue) || operators.Contains(rightValue))
            {
                throw new FormatException(
                    $"Left/Right value is an 'Operator' (AND, OR) instead of a value: '{leftValue}{rightValue}'");
            }
        }
    }
}