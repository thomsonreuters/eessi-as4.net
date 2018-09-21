using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using NLog;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class SqlServerDbCommand : IAS4DbCommand
    {
        private readonly DatastoreContext _context;

        // TODO: this is kind of similiar to the 'DatastoreTable' class
        private readonly IDictionary<string, Func<DatastoreContext, IQueryable<Entity>>> _tablesByName = 
            new Dictionary<string, Func<DatastoreContext, IQueryable<Entity>>>
            {
                {"InMessages", c => c.InMessages.FromSql(CreateSqlStatement("InMessages"))},
                {"OutMessages", c => c.OutMessages.FromSql(CreateSqlStatement("OutMessages"))},
                {"InExceptions", c => c.InExceptions.FromSql(CreateSqlStatement("InExceptions"))},
                {"OutExceptions", c => c.OutExceptions.FromSql(CreateSqlStatement("OutExceptions"))},
                {"RetryReliability", c => c.RetryReliability.FromSql(CreateSqlStatement("RetryReliability"))}
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDbCommand" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SqlServerDbCommand(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the exclusive lock isolation for the transaction of retrieval of entities.
        /// </summary>
        /// <value>The exclusive lock isolation.</value>
        public IsolationLevel? ExclusiveLockIsolation => new IsolationLevel?();

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
            if (!(DatastoreTable.IsTableNameKnown(tableName) && _tablesByName.ContainsKey(tableName)))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }

            return _tablesByName[tableName](_context)
                .Where(filter.Replace("\'", "\""))
                .OrderBy(x => x.InsertionTime)
                .Take(takeRows)
                .ToList();
        }

        private static string CreateSqlStatement(string tableName)
        {
            return $"SELECT * FROM {tableName} WITH (xlock, readpast)";
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
                                (m.EbmsMessageType = 'UserMessage' AND m.Status IN('Ack', 'Nack')) 
                                OR m.EbmsMessageType IN('Receipt', 'Error')
                             )"
                    : string.Empty;

            string sql =
                $"DELETE m FROM {tableName} m " +
                $"WHERE m.InsertionTime < GETDATE() - {retentionPeriod.TotalDays:##.##} " +
                $"AND Operation IN ({operations})" +
                outMessagesWhere;

#pragma warning disable EF1000 // Possible SQL injection vulnerability.
            // The DatastoreTable makes sure that we only use known table names.
            // The list of Operation enums makes sure that only use Operation values.
            // The TotalDays of the TimeSpan is an integer.
            int rows = _context.Database.ExecuteSqlCommand(sql);
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
            LogManager.GetCurrentClassLogger().Trace($"Cleaned {rows} row(s) for table '{tableName}'");
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
                "SELECT TOP 10 OutMessages.* "
                + "FROM OutMessages WITH (xlock, readpast) "
                + "INNER JOIN InMessages "
                + "ON OutMessages.EbmsRefToMessageId = InMessages.EbmsMessageId "
                + "WHERE OutMessages.Operation = 'ToBePiggyBacked' "
                + "AND OutMessages.URL = {0} "
                + "AND InMessages.MPC = {1} "
                + "AND OutMessages.EbmsMessageType != 'UserMessage' "
                + "ORDER BY OutMessages.InsertionTime DESC ";

            return _context.OutMessages
                           .FromSql(sql, url, mpc)
                           .AsEnumerable<OutMessage>();
        }
    }
}
