using System;
using System.Collections.Generic;
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

        private static ILogger Logger = LogManager.GetCurrentClassLogger();

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

            return DatastoreTable
                .FromTableName(tableName)(_context)
                .Where(filterExpression)
                .OrderBy(e => e.InsertionTime)
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

            string operations = string.Join(", ", allowedOperations.Select(x => "'" + x.ToString() + "'"));
            string outMessagesWhere =
                tableName.Equals("OutMessages")
                    ? @" AND (
                                (EbmsMessageType = 'UserMessage' AND Status IN('Ack', 'Nack')) 
                                OR EbmsMessageType IN('Receipt', 'Error')
                             )"
                    : string.Empty;

            // Sqlite doesn't allow JOIN statements in DELETE statements
            string retrySql =
                "DELETE FROM RetryReliability "
                + "WHERE Id IN ("
                    + $"SELECT m.Id FROM {tableName} m "
                    + $"WHERE m.InsertionTime<datetime('now', '-{retentionPeriod.TotalDays} day') "
                    + $"AND m.Operation IN ({operations}) "
                    + $"{outMessagesWhere})";

            string entitySql = 
                $"DELETE FROM {tableName} " +
                $"WHERE InsertionTime<datetime('now', '-{retentionPeriod.TotalDays} day') " +
                $"AND Operation IN ({operations}) " +
                outMessagesWhere;

#pragma warning disable EF1000 // Possible SQL injection vulnerability: 
            // The DatastoreTable makes sure that we only use known table names.
            // The list of Operation enums makes sure that only use Operation values.
            // The TotalDays of the TimeSpan is an integer.
            int retryRows = _context.Database.ExecuteSqlCommand(retrySql);
            int entityRows = _context.Database.ExecuteSqlCommand(entitySql);
#pragma warning restore EF1000 // Possible SQL injection vulnerability.

            Logger.Trace($"Cleaned {retryRows} row(s) for table 'RetryReliability'");
            Logger.Trace($"Cleaned {entityRows} row(s) for table '{tableName}'");
        }

        /// <summary>
        /// Selects in a reliable way the ToBePiggyBacked SignalMessages stored in the OutMessage table.
        /// </summary>
        /// <param name="url">The endpoint to which the OutMessage SignalMessage should be Piggy Backed.</param>
        /// <param name="mpc">The MPC of the incoming PullRequest to match on the related UserMessage of the Piggy Backed SignalMessage.</param>
        /// <returns></returns>
        public IEnumerable<OutMessage> SelectToBePiggyBackedSignalMessages(string url, string mpc)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (mpc == null)
            {
                throw new ArgumentNullException(nameof(mpc));
            }

            // TODO: the 'TOP' of the query should be configurable.
            const string sql =
                "SELECT OutMessages.* "
                + "FROM OutMessages "
                + "INNER JOIN InMessages "
                + "ON OutMessages.EbmsRefToMessageId = InMessages.EbmsMessageId "
                + "WHERE OutMessages.Operation = 'ToBePiggyBacked' "
                + "AND OutMessages.URL = {0} "
                + "AND InMessages.MPC = {1} "
                + "AND OutMessages.EbmsMessageType != 'UserMessage' "
                + "ORDER BY OutMessages.InsertionTime ASC "
                + "LIMIT 10";

            return _context.OutMessages
                           .FromSql(sql, url, mpc)
                           .AsEnumerable<OutMessage>();
        }
    }
}
