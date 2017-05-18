using System;
using System.Collections.Generic;
using System.Linq;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal sealed class EqualExpression
    {
        private static readonly IDictionary<string, Func<IEqualExpression>> Expressions
            = new Dictionary<string, Func<IEqualExpression>>
            {
                ["="] = () => new SameExpression(),
                ["IS"] = () => new SameExpression(),
                ["IS NOT"] = () => new NotSameExpression(),
                ["!="] = () => new NotSameExpression()
            };

        /// <summary>
        /// Create a <see cref="IEqualExpression"/> implementation for the given <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression symbol ('=', '!=').</param>
        /// <param name="databaseSet">The database Set.</param>
        /// <returns></returns>
        public static string Equals<T>(string expression, T databaseSet)
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

            return Expressions[separator]().Equals(left, right, databaseSet).ToString();
        }

        private static void ThrowIfInvalidEqualEspression(string left, string separator, string right)
        {
            if (left == null || left.Contains(" ") || right == null || right.Contains(" "))
            {
                throw new FormatException($"Equality expression is invalid: '{left}{separator}{right}'");
            }
        }
    }

    internal interface IEqualExpression
    {
        /// <summary>
        /// Verification if the given <paramref name="columnValue"/> for the given <paramref name="columnName"/> is the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        /// <param name="databaseSet"></param>
        /// <returns></returns>
        bool Equals<T>(string columnName, string columnValue, T databaseSet);
    }
}