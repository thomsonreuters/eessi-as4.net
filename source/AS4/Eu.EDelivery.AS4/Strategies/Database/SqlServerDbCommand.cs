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
                {"ReceptionAwareness", c => c.ReceptionAwareness.FromSql(CreateSqlStatement("ReceptionAwareness"))}
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

            int rows = _context.Database.ExecuteSqlCommand(sql);
            LogManager.GetCurrentClassLogger().Trace($"Cleaned {rows} row(s) for table '{tableName}'");
        }
    }
}
