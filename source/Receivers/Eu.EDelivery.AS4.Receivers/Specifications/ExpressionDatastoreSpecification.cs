using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Receivers.Specifications.Expressions;
using Expression = System.Linq.Expressions.Expression<System.Func<Eu.EDelivery.AS4.Common.DatastoreContext,
            System.Collections.Generic.IEnumerable<Eu.EDelivery.AS4.Entities.Entity>>>;

namespace Eu.EDelivery.AS4.Receivers.Specifications
{
    internal class ExpressionDatastoreSpecification : IDatastoreSpecification
    {
        private DatastoreSpecificationArgs _arguments;

        public string FriendlyExpression => $"FROM {_arguments.TableName} WHERE {_arguments.Filter}";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionDatastoreSpecification"/> class.
        /// </summary>
        /// <param name="args">Arguments to build the expression.</param>
        public void Configure(DatastoreSpecificationArgs args)
        {
            _arguments = args;
        }

        /// <summary>
        /// Gets the Expression for which the <see cref="DatastoreReceiver"/> must search for messages.
        /// </summary>
        /// <returns></returns>
        public Expression GetExpression()
        {
            ThrowIfInvalidFilter(_arguments.Filter);

            return x => GetExpression(x);
        }

        private static void ThrowIfInvalidFilter(string filter)
        {
            bool expressionHasntEqualOpenAndClosingParenthesis =
                filter.Count(c => c.Equals('(')) != filter.Count(c => c.Equals(')'));

            if (expressionHasntEqualOpenAndClosingParenthesis)
            {
                throw new FormatException("Expression doesn't contain as much '(' as ')'");
            }
        }

        private IEnumerable<Entity> GetExpression(DatastoreContext datastoreContext)
        {
            object tableProperty = datastoreContext
               .GetType()
               .GetProperty(_arguments.TableName)
               .GetValue(datastoreContext);

            return GetEntities(tableProperty as IQueryable<Entity>);
        }

        private IEnumerable<T> GetEntities<T>(IQueryable<T> queryable)
        {
            IQueryable<T> query = queryable.Where(dbSet => Where(dbSet));

            if (_arguments.TakeRecords > 0)
            {
                query = query.Take(_arguments.TakeRecords);
            }

            return query.ToList();
        }

        private bool Where<T>(T databaseSet)
        {
            var tokens = new Stack<Token>();

            foreach (Token token in Token.Tokenize(_arguments.Filter))
            {
                if (token.IsClosedParenthesis)
                {
                    tokens.Push(EvaluateEverythingInsideParenthesis(tokens));
                }
                else
                {
                    tokens.Push(
                       token.IsEqualExpression
                           ? Token.CreateToken(EqualExpression.Equals(token.Expression, databaseSet))
                           : token);
                }
            }

            return LogicalEval(tokens.ToArray()).Evaluate;
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
                .For(operatorValue)
                .Evaluate(leftValue, rightValue)
                .ToString();

            return Token.CreateToken(expression);
        }
    }
}
