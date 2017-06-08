namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    /// <summary>
    /// <see cref="IExpression"/> implementation to evaluate a boolean 'OR'.
    /// </summary>
    internal sealed class OrExpression : IExpression
    {
        private readonly bool _left;
        private readonly bool _right;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrExpression"/> class.
        /// </summary>
        /// <param name="left">if set to <c>true</c> [left].</param>
        /// <param name="right">if set to <c>true</c> [right].</param>
        public OrExpression(bool left, bool right)
        {
            this._left = left;
            this._right = right;
        }

        /// <summary>
        /// Evaluate the expression.
        /// </summary>
        /// <returns></returns>
        public bool Evaluate()
        {
            return _left || _right;
        }
    }
}
