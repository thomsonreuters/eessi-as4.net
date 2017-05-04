using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
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
            return x => GetExpression(x);
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
            var stack = new Stack<string>();

            foreach (string token in Tokenize(_arguments.Filter))
            {
                bool isParenthesisClosed = token.Equals(")");
                if (isParenthesisClosed)
                {
                    stack.Push(EvaluateEverythingInsideParenthesis(stack));
                }
                else
                {
                    bool isEqualExpression = token.Contains("=");
                    stack.Push(isEqualExpression ? EqualEvaluate(token, databaseSet) : token);
                }
            }

            bool.TryParse(LogicalEval(stack.ToArray()), out var outcome);
            return outcome;
        }

        private static IEnumerable<string> Tokenize(string filter)
        {
            return
                Regex.Split(filter, @"(AND|OR|\(|\))")
                     .Select(t => t.Trim())
                     .Where(t => !string.IsNullOrEmpty(t))
                     .ToArray();
        }

        private static string EvaluateEverythingInsideParenthesis(Stack<string> stack)
        {
            var everythingInParenthesis = new List<string>();
            string nextToken = stack.Pop();

            while (!nextToken.Equals("("))
            {
                everythingInParenthesis.Add(nextToken);
                nextToken = stack.Pop();
            }

            return LogicalEval(everythingInParenthesis.ToArray());
        }

        private static string LogicalEval(IEnumerable<string> expression)
        {
            var stack = new Stack<string>();

            foreach (string token in expression)
            {
                stack.Push(token);

                if (stack.Count == 3)
                {
                    stack.Push(EvaluateBooleanExpression(stack));
                }
            }

            return stack.Pop();
        }

        private static string EvaluateBooleanExpression(Stack<string> stack)
        {
            bool.TryParse(stack.Pop(), out var left);
            bool.TryParse(stack.Pop(), out var right);

            return LogicalExpressions[stack.Pop()](left, right).ToString();
        }

        private static readonly Dictionary<string, Func<bool, bool, bool>> LogicalExpressions =
            new Dictionary<string, Func<bool, bool, bool>>
            {
                ["AND"] = (a, b) => a && b,
                ["OR"] = (a, b) => a || b
            };

        private static string EqualEvaluate<T>(string expression, T databaseSet)
        {
            string separator = expression.Contains("!=") ? "!=" : "=";
            string[] entries =
                expression.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(e => e.Trim())
                          .ToArray();

            string left = entries[0];
            string right = entries[1];

            return EqualExpressions[separator](left, right, databaseSet).ToString();
        }

        private static readonly Dictionary<string, Func<string, string, object, bool>> EqualExpressions =
            new Dictionary<string, Func<string, string, object, bool>>
            {
                ["!="] = (columnName, columnValue, databaseSet) => !IsEqual(columnName, columnValue, databaseSet),
                ["="] = (columnName, columnValue, databaseSet) => IsEqual(columnName, columnValue, databaseSet)
            };

        private static bool IsEqual<T>(string columnName, string columnValue, T databaseSet)
        {
            PropertyInfo filterPropertyInfo = databaseSet.GetType().GetProperty(columnName);

            object propertyValue = filterPropertyInfo.GetValue(databaseSet);
            object configuredValue = ParseConfiguredValue(propertyValue, columnValue);

            return propertyValue.Equals(configuredValue);
        }

        private static object ParseConfiguredValue(object propertyValue, string columnValue)
        {
            return Conversions.FirstOrDefault(c => c.Key(propertyValue)).Value(propertyValue, columnValue);
        }

        private static readonly Dictionary<Func<object, bool>, Func<object, string, object>> Conversions =
            new Dictionary<Func<object, bool>, Func<object, string, object>>
            {
                [p => p.GetType().IsEnum] = (a, b) => Enum.Parse(a.GetType(), b),
                [p => p is int] = (a, b) => Convert.ToInt32(b),
                [p => p is string] = (a, b) => b,
                [p => true] = (a, b) => default(object)
            };
    }
}
