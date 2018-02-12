using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class InMemoryDbCommand : IAS4DbCommand
    {
        private readonly DatastoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDbCommand" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public InMemoryDbCommand(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        public async Task CreateDatabase()
        {
            await _context.Database.EnsureCreatedAsync();
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

            return DatastoreTable.FromTableName(tableName)(_context)
                .Where(filterExpression)
                .ToList();
        }

        /// <summary>
        /// Delete the Messages Entities that are inserted passed a given <paramref name="retentionPeriod"/> 
        /// and has a <see cref="Operation"/> within the given <paramref name="allowedOperations"/>.
        /// </summary>
        /// <param name="retentionPeriod">The retention period.</param>
        /// <param name="allowedOperations">The allowed operations.</param>
        public void BatchDeleteMessagesOverRetentionPeriod(
            TimeSpan retentionPeriod,
            IEnumerable<Operation> allowedOperations)
        {
            // TODO: needs to be implemented?

            foreach (string table in DatastoreTable.MessageTables)
            {
                IQueryable<Entity> entities = 
                    DatastoreTable.FromTableName(table)(_context)
                                  .Where(x => x.InsertionTime < DateTimeOffset.UtcNow.Subtract(retentionPeriod));

                _context.RemoveRange(entities);
                _context.SaveChanges();
            }
        }
    }
}