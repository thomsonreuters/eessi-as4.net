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
            IEnumerable<Token> tokens = Token.Tokenize(_arguments.Filter);
            IQueryable<T> query = queryable.Where(dbSet => DatastoreExpressionParser.Evaluate(dbSet, tokens));

            if (_arguments.TakeRecords > 0)
            {
                query = query.Take(_arguments.TakeRecords);
            }

            return query.ToList();
        }
    }
}
