using System;
using System.Collections.Generic;
using System.Linq;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal sealed class BooleanExpression
    {
        private static readonly IDictionary<string, Func<IBooleanExpression>> LogicalExpressions =
            new Dictionary<string, Func<IBooleanExpression>>
            {
                ["AND"] = () => new AndExpression(),
                ["OR"] = () => new OrExpression()
            };

        private readonly IBooleanExpression _innerExpression;

        private BooleanExpression(IBooleanExpression expression)
        {
            _innerExpression = expression;
        }

        /// <summary>
        /// Evaluate a given expression arguments to a boolean expression result.
        /// </summary>
        /// <param name="leftValue">The left argument.</param>
        /// <param name="rightValue">The right argument</param>
        /// <returns></returns>
        public bool Evaluate(string leftValue, string rightValue)
        {
            ThrowIfInvalidBooleanExpression(leftValue, rightValue);

            bool.TryParse(leftValue, out bool left);
            bool.TryParse(rightValue, out bool right);

            return _innerExpression.Evaluate(left, right);
        }

        /// <summary>
        /// Create a new <see cref="BooleanExpression" />
        /// </summary>
        /// <param name="operatorValue"></param>
        /// <returns></returns>
        public static BooleanExpression For(string operatorValue)
        {
            IBooleanExpression expression = LogicalExpressions[operatorValue]();

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

    public interface IBooleanExpression
    {
        /// <summary>
        /// Evaluate a given expression arguments to a boolean expression result.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument</param>
        /// <returns></returns>
        bool Evaluate(bool left, bool right);
    }
}