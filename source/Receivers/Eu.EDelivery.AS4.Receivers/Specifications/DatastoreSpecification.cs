using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using NLog;
using Expression = System.Linq.Expressions.Expression<System.Func<Eu.EDelivery.AS4.Common.DatastoreContext,
            System.Collections.Generic.IEnumerable<Eu.EDelivery.AS4.Entities.Entity>>>;

namespace Eu.EDelivery.AS4.Receivers.Specifications
{
    /// <summary>
    /// Specification to define <see cref="Expression"/> Models
    /// </summary>
    internal class DatastoreSpecification : IDatastoreSpecification
    {
        private DatastoreSpecificationArgs _arguments;

        private PropertyInfo _filterPropertyInfo;

        /// <summary>
        /// Configure the given <see cref="DatastoreSpecification"/>
        /// </summary>
        /// <param name="args"></param>
        public void Configure(DatastoreSpecificationArgs args)
        {
            _arguments = args;
        }

        /// <summary>
        /// Gets the friendly expression of the specification.
        /// </summary>
        public string FriendlyExpression => $"FROM {_arguments.TableName} WHERE {_arguments.Filter} == {_arguments.FilterValue}";

        /// <summary>
        /// Get the Expression for the <see cref="DatastoreContext"/>
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
            string name = _arguments.Filter;

            if (_filterPropertyInfo == null)
            {
                _filterPropertyInfo = databaseSet.GetType().GetProperty(name);
            }

            if (_filterPropertyInfo == null)
            {
                LogManager.GetCurrentClassLogger().Error($"FilterColumn {_arguments.Filter} on DatastoreReceiver could not be found.");
                return false;
            }

            object propertyValue = _filterPropertyInfo.GetValue(databaseSet);
            object configuredValue = ParseConfiguredValue(propertyValue);

            return propertyValue.Equals(configuredValue);
        }

        private object ParseConfiguredValue(object propertyValue)
        {
            string value = _arguments.FilterValue;

            if (propertyValue.GetType().IsEnum)
            {
                return Enum.Parse(propertyValue.GetType(), value);
            }

            if (propertyValue is bool)
            {
                return Convert.ToBoolean(value);
            }

            return default(object);
        }
    }

    internal class DatastoreSpecificationArgs
    {
        public string TableName { get; }
        public string Filter { get; }
        public string FilterValue { get; }

        public int TakeRecords { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreSpecificationArgs"/> class.
        /// </summary>
        /// <param name="tableName">The table Name.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="take">The take.</param>
        public DatastoreSpecificationArgs(string tableName, string filter, int take = 20)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("A tablename should be specified.", nameof(tableName));
            }

            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentException("A column where to filter on should be specified.", nameof(filter));
            }

            TableName = tableName;
            Filter = filter;
            TakeRecords = take;
        }

        /// <summary>Initializes a new instance of the <see cref="DatastoreSpecificationArgs"/> class.</summary>
        /// <param name="tableName">The table Name.</param>
        /// <param name="filterColumn">The filter Column.</param>
        /// <param name="filterValue">The filter Value.</param>
        /// <param name="take">The take.</param>
        public DatastoreSpecificationArgs(string tableName, string filterColumn, string filterValue, int take) : this(tableName, filterColumn, take)
        {
            if (string.IsNullOrWhiteSpace(filterValue))
            {
                throw new ArgumentException("A filtervalue should be specified.", nameof(filterValue));
            }

            FilterValue = filterValue;
        }
    }
}