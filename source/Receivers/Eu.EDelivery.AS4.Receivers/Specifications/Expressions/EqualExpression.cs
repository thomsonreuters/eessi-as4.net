using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal sealed class EqualExpression<TSet> : IExpression
    {
        private static readonly IDictionary<string, Func<string, string, TSet, IExpression>> Expressions =
            new Dictionary<string, Func<string, string, TSet, IExpression>>
            {
                ["="] = (name, value, set) => new SameExpression<TSet>(name, value, set),
                ["IS"] = (name, value, set) => new SameExpression<TSet>(name, value, set),
                ["IS NOT"] = (name, value, set) => new NotSameExpression<TSet>(name, value, set),
                ["!="] = (name, value, set) => new NotSameExpression<TSet>(name, value, set)
            };

        private readonly IExpression _innerExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualExpression{TSet}" /> class.
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
        public static EqualExpression<TSet> For(string expression, TSet databaseSet)
        {
            string separator = expression.Contains("=")
                                   ? expression.Contains("!=") ? "!=" : "="
                                   : expression.Contains(" IS NOT ") ? "IS NOT" : "IS";

            string[] entries =
                expression.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries)
                          .Select(e => e.Trim())
                          .ToArray();

            string left = entries.ElementAtOrDefault(0);
            string right = entries.ElementAtOrDefault(1);

            ThrowIfInvalidEqualEspression(left, separator, right);

            return new EqualExpression<TSet>(Expressions[separator](left, right, databaseSet));
        }

        private static void ThrowIfInvalidEqualEspression(string left, string separator, string right)
        {
            if (left == null || left.Contains(" ") || right == null || right.Contains(" "))
            {
                throw new FormatException($"Equality expression is invalid: '{left}{separator}{right}'");
            }
        }

        /// <summary>
        /// Evaluates if an expression is has the same left and right side value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <seealso cref="IExpression" />
        private sealed class SameExpression<T> : IExpression
        {
            private readonly string _columnName;
            private readonly string _columnValue;
            private readonly T _databaseSet;

            /// <summary>
            /// Initializes a new instance of the <see cref="SameExpression{T}" /> class.
            /// </summary>
            /// <param name="columnName">Name of the column.</param>
            /// <param name="columnValue">The column value.</param>
            /// <param name="databaseSet">The database set.</param>
            public SameExpression(string columnName, string columnValue, T databaseSet)
            {
                _columnName = columnName;
                _columnValue = columnValue;
                _databaseSet = databaseSet;
            }

            /// <summary>
            /// Evaluate the expression.
            /// </summary>
            /// <returns></returns>
            public bool Evaluate()
            {
                PropertyInfo filterPropertyInfo = _databaseSet.GetType().GetProperty(_columnName);

                object propertyValue = filterPropertyInfo.GetValue(_databaseSet);
                object configuredValue = Conversion.Convert(propertyValue, _columnValue);

                return propertyValue?.Equals(configuredValue) == true || propertyValue == configuredValue;
            }
        }

        /// <summary>
        /// <see cref="IExpression" /> implementation to verify if the column value is not the same as the given value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private sealed class NotSameExpression<T> : IExpression
        {
            private readonly IExpression _innerExpression;

            /// <summary>
            /// Initializes a new instance of the <see cref="NotSameExpression{T}" /> class.
            /// </summary>
            /// <param name="columnName">Name of the column.</param>
            /// <param name="columnValue">The column value.</param>
            /// <param name="databaseSet">The database set.</param>
            public NotSameExpression(string columnName, string columnValue, T databaseSet)
            {
                _innerExpression = new SameExpression<T>(columnName, columnValue, databaseSet);
            }

            /// <summary>
            /// Evaluate the expression.
            /// </summary>
            /// <returns></returns>
            public bool Evaluate()
            {
                return !_innerExpression.Evaluate();
            }
        }
    }
}