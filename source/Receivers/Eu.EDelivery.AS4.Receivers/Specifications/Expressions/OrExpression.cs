namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    /// <summary>
    /// <see cref="IBooleanExpression"/> implementation to evaluate a boolean 'OR'.
    /// </summary>
    internal sealed class OrExpression : IBooleanExpression
    {
        /// <summary>
        /// Evaluate a given expression arguments to a boolean expression result.
        /// </summary>
        /// <param name="left">The left argument.</param>
        /// <param name="right">The right argument</param>
        /// <returns></returns>
        public bool Evaluate(bool left, bool right)
        {
            return left || right;
        }
    }
}
