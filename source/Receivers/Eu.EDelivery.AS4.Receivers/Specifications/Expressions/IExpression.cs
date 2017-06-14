namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    public interface IExpression
    {
          /// <summary>
          /// Evaluate the expression.
          /// </summary>
          /// <returns></returns>
          bool Evaluate();
    }
}