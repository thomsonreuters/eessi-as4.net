using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Strategies.Database;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    internal class InMemoryDbCommand : IAS4DbCommand
    {
        private readonly IQueryable<Entity> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDbCommand" /> class.
        /// </summary>
        /// <param name="dbSet">The database set.</param>
        public InMemoryDbCommand(IQueryable<Entity> dbSet)
        {
            _dbSet = dbSet;
        }

        /// <summary>
        /// Exclusively retrieves the entities.
        /// </summary>
        /// <param name="tableName">Name of the Db table.</param>
        /// <param name="filter">Order by this field.</param>
        /// <param name="takeRows">Take this amount of rows.</param>
        /// <returns></returns>
        public IEnumerable<Entity> ExclusivelyRetrieveEntities(string tableName, string filter, int takeRows)
        {
            string filterExpression = filter.Replace("\'", "\"");

            return _dbSet.Where(filterExpression)
                         .ToList();
        }
    }
}