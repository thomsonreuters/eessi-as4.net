namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    /// <summary>
    /// <see cref="IExpression"/> implementation to evaluate a boolean 'AND'.
    /// </summary>
    internal sealed class AndExpression : IExpression
    {
        private readonly bool _left;
        private readonly bool _right;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndExpression" /> class.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        public AndExpression(bool left, bool right)
        {
            _left = left;
            _right = right;
        }

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <returns></returns>
        public bool Evaluate()
        {
            return _left && _right;
        }
    }
}
