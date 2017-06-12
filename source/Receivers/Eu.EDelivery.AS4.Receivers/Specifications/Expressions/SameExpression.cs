using System.ComponentModel;
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
        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        public static object Convert(object property, string value)
        {
            if (value?.Equals("NULL") == true)
            {
                return null;
            }

            return TypeDescriptor.GetConverter(property).ConvertFrom(value);
        }
    }
}
