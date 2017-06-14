using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Receivers.Specifications.Expressions
{
    internal static class DatastoreExpressionEvaluator
    {
        /// <summary>
        /// Evaluates the specified database set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="databaseSet">The database set.</param>
        /// <param name="tokenizedExpression">The token expression.</param>
        /// <returns></returns>
        public static bool Evaluate<T>(T databaseSet, IEnumerable<Token> tokenizedExpression)
        {
            var tokenResult = new Stack<Token>();

            foreach (Token token in tokenizedExpression)
            {
                if (token.IsClosedParenthesis)
                {
                    tokenResult.Push(EvaluateEverythingInsideParenthesis(tokenResult));
                }
                else
                {
                    tokenResult.Push(
                       token.IsEqualExpression
                           ? Token.CreateToken(EqualExpression<T>.For(token.Expression, databaseSet).Evaluate().ToString())
                           : token);
                }
            }

            return LogicalEval(tokenResult.ToArray()).Evaluate;
        }

        private static Token EvaluateEverythingInsideParenthesis(Stack<Token> stack)
        {
            var everythingInParenthesis = new List<Token>();
            Token nextToken = stack.Pop();

            while (!nextToken.IsOpenParenthesis)
            {
                everythingInParenthesis.Add(nextToken);
                nextToken = stack.Pop();
            }

            return LogicalEval(everythingInParenthesis.ToArray());
        }

        private static Token LogicalEval(IEnumerable<Token> expression)
        {
            var stack = new Stack<Token>();

            foreach (Token token in expression)
            {
                stack.Push(token);

                if (stack.Count == 3)
                {
                    stack.Push(EvaluateBooleanExpression(stack));
                }
            }

            return stack.Pop();
        }

        private static Token EvaluateBooleanExpression(Stack<Token> stack)
        {
            string leftValue = stack.Pop().Expression;
            string operatorValue = stack.Pop().Expression;
            string rightValue = stack.Pop().Expression;

            string expression = BooleanExpression
                .For(operatorValue, leftValue, rightValue)
                .Evaluate()
                .ToString();

            return Token.CreateToken(expression);
        }
    }
}
