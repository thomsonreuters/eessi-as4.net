namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    /// <summary>
    ///   <see cref="IExpression" /> implementation to verify if the column value is not the same as the given value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class NotSameExpression<T> : IExpression
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