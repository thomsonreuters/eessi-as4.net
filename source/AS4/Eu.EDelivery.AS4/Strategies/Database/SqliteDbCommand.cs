using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class SqliteDbCommand : IAS4DbCommand
    {
        private readonly DatastoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbCommand" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SqliteDbCommand(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the exclusive lock isolation for the transaction of retrieval of entities.
        /// </summary>
        /// <value>The exclusive lock isolation.</value>
        public IsolationLevel? ExclusiveLockIsolation => IsolationLevel.Serializable;

        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        public async Task CreateDatabase()
        {
            await _context.Database.MigrateAsync();
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
            DatastoreTable.EnsureTableNameIsKnown(tableName);

            string filterExpression = filter.Replace("\'", "\"");

            return DatastoreTable.FromTableName(tableName)(_context)
                .Where(filterExpression)
                .OrderBy(x => x.InsertionTime)
                .Take(takeRows)
                .ToList();
        }

        /// <summary>
        /// Delete the Messages Entities that are inserted passed a given <paramref name="retentionPeriod"/> 
        /// and has a <see cref="Operation"/> within the given <paramref name="allowedOperations"/>.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="retentionPeriod">The retention period.</param>
        /// <param name="allowedOperations">The allowed operations.</param>
        public void BatchDeleteOverRetentionPeriod(
            string tableName,
            TimeSpan retentionPeriod,
            IEnumerable<Operation> allowedOperations)
        {
            DatastoreTable.EnsureTableNameIsKnown(tableName);

            string receptionAwarenessJoin =
                tableName.Equals("OutMessages")
                    ? "AND Id IN (" +
                      "  SELECT m.Id" +
                      "  FROM OutMessages m" +
                      "  LEFT OUTER JOIN ReceptionAwareness r" +
                      "  ON r.RefToOutMessageId = m.Id" +
                      "  WHERE r.Status = 'Completed' OR r.Status IS NULL)"
                    : string.Empty;

            string operations = string.Join(", ", allowedOperations.Select(x => "'" + x.ToString() + "'"));
            string command =
                $"DELETE FROM {tableName} " +
                $"WHERE InsertionTime<datetime('now', '-{retentionPeriod.TotalDays} day') " +
                $"AND Operation IN({operations}) " +
                receptionAwarenessJoin;

            int rows = _context.Database.ExecuteSqlCommand(command);

            LogManager.GetCurrentClassLogger().Debug($"Cleaned {rows} row(s) for table '{tableName}'");
        }
    }
}
