namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    /// <summary>
    /// <see cref="IEqualExpression"/> implementation to verify if the column value is not the same as the given value. 
    /// </summary>
    internal sealed class NotSameExpression : IEqualExpression
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
            return !new SameExpression().Equals(columnName, columnValue, databaseSet);
        }
    }
}