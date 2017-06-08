using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    /// <summary>
    /// Evaluates if an expression is has the same left and right side value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IExpression" />
    internal sealed class SameExpression<T> : IExpression
    {
        private readonly T _databaseSet;
        private readonly string _columnName;
        private readonly string _columnValue;

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

    internal static class Conversion
    {
        private static readonly Dictionary<Func<object, string, bool>, Func<object, string, object>> Conversions =
            new Dictionary<Func<object, string, bool>, Func<object, string, object>>
            {
                [(a, b) => b.Equals("NULL")] = (a, b) => null,
                [(a, b) => a?.GetType().IsEnum == true] = (a, b) => Enum.Parse(a.GetType(), b),
                [(a, b) => a is int] = (a, b) => System.Convert.ToInt32(b),
                [(a, b) => a is string] = (a, b) => b,
                [(a, b) => true] = (a, b) => default(object)
            };

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static object Convert(object property, string value)
        {
            return Conversions.FirstOrDefault(c => c.Key(property, value)).Value(property, value);
        }
    }
}
