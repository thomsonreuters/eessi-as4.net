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
    internal class DatastoreSpecification
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
            string name = _arguments.FilterColumnName;

            if (_filterPropertyInfo == null)
            {
                _filterPropertyInfo = databaseSet.GetType().GetProperty(name);
            }

            if (_filterPropertyInfo == null)
            {
                LogManager.GetCurrentClassLogger().Error($"FilterColumn {_arguments.FilterColumnName} on DatastoreReceiver could not be found.");
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

            return default(object);
        }
    }

    internal class DatastoreSpecificationArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreSpecificationArgs" /> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="filterColumn">The filter column.</param>
        /// <param name="filterValue">The filter value.</param>
        /// <param name="take">The take.</param>
        /// <exception cref="ArgumentException">
        /// A tablename should be specified. - tableName
        /// or
        /// A column where to filter on should be specified. - filterColumn
        /// or
        /// A filtervalue should be specified. - filterValue
        /// </exception>
        public DatastoreSpecificationArgs(string tableName, string filterColumn, string filterValue, int take)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("A tablename should be specified.", nameof(tableName));
            }

            if (string.IsNullOrWhiteSpace(filterColumn))
            {
                throw new ArgumentException("A column where to filter on should be specified.", nameof(filterColumn));
            }

            if (string.IsNullOrWhiteSpace(filterValue))
            {
                throw new ArgumentException("A filtervalue should be specified.", nameof(filterValue));
            }

            TableName = tableName;
            FilterColumnName = filterColumn;
            FilterValue = filterValue;
            TakeRecords = take;
        }

        public string TableName { get; }

        public string FilterColumnName { get; }

        public string FilterValue { get; }

        public int TakeRecords { get; }
    }
}