using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Dynamic.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class SqliteDbCommand : IAS4DbCommand
    {
        private readonly IQueryable<Entity> _dbSet;
        private readonly DatastoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbCommand" /> class.
        /// </summary>
        /// <param name="dbSet">The database set.</param>
        /// <param name="context">The context.</param>
        public SqliteDbCommand(IQueryable<Entity> dbSet, DatastoreContext context)
        {
            _dbSet = dbSet;
            _context = context;
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
            if (!TableValidation.IsTableNameKnown(tableName))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }

            _context.Database.ExecuteSqlCommand("BEGIN EXCLUSIVE");
            string filterExpression = filter.Replace("\'", "\"");

            return _dbSet.Where(filterExpression)
                         .OrderBy(x => x.InsertionTime)
                         .Take(takeRows)
                         .ToList();
        }
    }
}
