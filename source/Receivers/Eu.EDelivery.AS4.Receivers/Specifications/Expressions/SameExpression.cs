using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    /// <summary>
    /// <see cref="IEqualExpression"/> implementation to verify if the column value is the same as the given value.
    /// </summary>
    internal sealed class SameExpression : IEqualExpression
    {
        /// <summary>
        /// Verification if the given <paramref name="columnValue"/> for the given <paramref name="columnName"/> is the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        /// <param name="databaseSet"></param>
        /// <returns></returns>
        public bool Equals<T>(string columnName, string columnValue, T databaseSet)
        {
            PropertyInfo filterPropertyInfo = databaseSet.GetType().GetProperty(columnName);

            object propertyValue = filterPropertyInfo.GetValue(databaseSet);
            object configuredValue = Conversion.Convert(propertyValue, columnValue);

            return propertyValue.Equals(configuredValue);
        }
    }

    internal static class Conversion
    {
        private static readonly Dictionary<Func<object, bool>, Func<object, string, object>> Conversions =
            new Dictionary<Func<object, bool>, Func<object, string, object>>
            {
                [p => p.GetType().IsEnum] = (a, b) => Enum.Parse(a.GetType(), b),
                [p => p is int] = (a, b) => System.Convert.ToInt32(b),
                [p => p is string] = (a, b) => b,
                [p => true] = (a, b) => default(object)
            };

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static object Convert(object property, string value)
        {
            return Conversions.FirstOrDefault(c => c.Key(property)).Value(property, value);
        }
    }
}
