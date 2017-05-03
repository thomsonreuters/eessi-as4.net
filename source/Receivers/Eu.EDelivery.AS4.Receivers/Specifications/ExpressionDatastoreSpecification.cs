using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Expression = System.Linq.Expressions.Expression<System.Func<
    Eu.EDelivery.AS4.Common.DatastoreContext,
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
            string[] ands = _arguments.Filter.Split(new[] {" AND "}, StringSplitOptions.RemoveEmptyEntries);

            bool? globalExpression =
                ands.Select(and => and.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries))
                    .Select(entries => WhereSingle(databaseSet, entries[0].Trim(), entries[1].Trim()))
                    .Aggregate<bool, bool?>(
                        seed: null,
                        func: (current, entryExpression) =>
                            current.HasValue ? entryExpression && current.Value : entryExpression);

            return globalExpression ?? false;
        }

        private static bool WhereSingle<T>(T databaseSet, string columnName, string columnValue)
        {
            PropertyInfo filterPropertyInfo = databaseSet.GetType().GetProperty(columnName);

            object propertyValue = filterPropertyInfo.GetValue(databaseSet);
            object configuredValue = ParseConfiguredValue(propertyValue, columnValue);

            return propertyValue.Equals(configuredValue);
        }

        /// <summary>
        /// Locks a given <paramref name="entity"/> with a given <paramref name="updateExpression"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="updateExpression"></param>
        public void LockEntity(Entity entity, string updateExpression)
        {
            string[] entries = updateExpression.Split(new[] {" = "}, StringSplitOptions.RemoveEmptyEntries);
            PropertyInfo property = entity.GetType().GetProperty(entries[0].Trim());
            object configuredValue = ParseConfiguredValue(property, entries[1].Trim());

            property.SetValue(entity, configuredValue);
        }

        private static object ParseConfiguredValue(object propertyValue, string columnValue)
        {
            if (propertyValue.GetType().IsEnum)
            {
                return Enum.Parse(propertyValue.GetType(), columnValue);
            }

            return default(object);
        }
    }
}
