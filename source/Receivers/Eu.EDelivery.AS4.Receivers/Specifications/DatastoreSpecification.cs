using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

using Expression = System.Linq.Expressions.Expression<System.Func<
    Eu.EDelivery.AS4.Common.DatastoreContext,
    System.Collections.Generic.IEnumerable<Eu.EDelivery.AS4.Entities.Entity>>>;

namespace Eu.EDelivery.AS4.Receivers.Specifications
{
    /// <summary>
    /// Specification to define <see cref="Expression"/> Models
    /// </summary>
    public class DatastoreSpecification
    {
        private IDictionary<string, string> _properties;

        /// <summary>
        /// Configure the given <see cref="DatastoreSpecification"/>
        /// </summary>
        /// <param name="properties"></param>
        public void Configure(IDictionary<string, string> properties)
        {
            this._properties = properties;
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
                .GetProperty(this._properties["Table"])
                .GetValue(datastoreContext);

            return GetEntities(tableProperty as IQueryable<Entity>);
        }

        private IEnumerable<T> GetEntities<T>(IQueryable<T> queryable)
        {
            return queryable.Where(dbSet => Where(dbSet)).ToList();
        }

        private bool Where<T>(T dbSet)
        {
            string name = this._properties["Field"];
            
            object propertyValue = dbSet.GetType().GetProperty(name).GetValue(dbSet);
            object configuredValue = ParseConfiguredValue(propertyValue);

            return propertyValue.Equals(configuredValue);
        }

        private object ParseConfiguredValue(object propertyValue)
        {
            string value = this._properties["Value"];

            if (propertyValue.GetType().IsEnum)
                return Enum.Parse(propertyValue.GetType(), value);

            if (propertyValue is bool)
                return Convert.ToBoolean(value);

            return default(object);
        }
    }
}