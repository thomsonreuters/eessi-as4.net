using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal sealed class Token
    {
        private Token(string expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Gets the inner expressions of the <see cref="Token"/>.
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// Gets the value indicating if the <see cref="Token"/> is a closed paranthesis ')'.
        /// </summary>
        public bool IsClosedParenthesis => Expression.Equals(")");

        /// <summary>
        /// Gets the value indicating if the <see cref="Token"/> is a open paranthesis '('.
        /// </summary>
        public bool IsOpenParenthesis => Expression.Equals("(");

        /// <summary>
        /// Gets the value indicating if the <see cref="Token"/> is a a equal expression '='.
        /// </summary>
        public bool IsEqualExpression => Expression.Contains("=");

        /// <summary>
        /// Gets the value indicating the outcome of the <see cref="Token"/> expression.
        /// </summary>
        public bool Evaluate
        {
            get
            {
                bool.TryParse(Expression, out var outcome);
                return outcome;
            }
        }

        /// <summary>
        /// Create all <see cref="Token"/> models for a given <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<Token> Tokenize(string filter)
        {
            return
                Regex.Split(filter, @"(AND|OR|\(|\))")
                     .Select(t => t.Trim())
                     .Where(t => !string.IsNullOrEmpty(t))
                     .ToArray()
                     .Select(CreateToken);
        }

        internal static Token CreateToken(string expression)
        {
            var token = new Token(expression);
            Validate(token);

            return token;
        }

        private static void Validate(Token token)
        {
            string expression = token.Expression;
            bool expressionHasToTheRequiredEqualSigns = expression.Count(c => c.Equals('=')) != 1;

            if (expressionHasToTheRequiredEqualSigns && !new[] { "AND", "OR", "(", ")", "True", "False" }.Contains(expression))
            {
                throw new FormatException($"Expression has invalid token: {expression}");
            }
        }
    }
}
