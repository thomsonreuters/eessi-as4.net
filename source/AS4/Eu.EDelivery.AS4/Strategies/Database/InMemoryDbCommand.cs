using System;
using System.Collections.Generic;
using System.Data;
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

        private static readonly IDictionary<string, Func<Entity, string>> GetOperationString = 
            new Dictionary<string, Func<Entity, string>>
            {
                ["OutMessages"] = e => (e as OutMessage)?.Operation,
                ["InMessages"] = e => (e as InMessage)?.Operation,
                ["OutExceptions"] = e => (e as OutException)?.Operation,
                ["InExceptions"] = e => (e as InException)?.Operation
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDbCommand" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public InMemoryDbCommand(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the exclusive lock isolation for the transaction of retrieval of entities.
        /// </summary>
        /// <value>The exclusive lock isolation.</value>
        public IsolationLevel? ExclusiveLockIsolation => new IsolationLevel();

        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        public async Task CreateDatabase()
        {
            await _context.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Wraps the given <paramref name="funcToWrap"/> into a DBMS storage type specific transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcToWrap">The function to wrap.</param>
        /// <returns></returns>
        public T WithTransaction<T>(Func<DatastoreContext, T> funcToWrap)
        {
            return funcToWrap(_context);
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
        /// <param name="tableName"></param>
        /// <param name="retentionPeriod">The retention period.</param>
        /// <param name="allowedOperations">The allowed operations.</param>
        public void BatchDeleteOverRetentionPeriod(
            string tableName,
            TimeSpan retentionPeriod,
            IEnumerable<Operation> allowedOperations)
        {
            IQueryable<Entity> entities =
                DatastoreTable.FromTableName(tableName)(_context)
                              .Where(x => x.InsertionTime < DateTimeOffset.UtcNow.Subtract(retentionPeriod)
                                          && allowedOperations.Contains(
                                              OperationUtils.Parse(
                                                  GetOperationString[tableName](x) ??
                                                  Operation.NotApplicable.ToString())));

            if (tableName.Equals("OutMessages"))
            {
                string[] ebmsMessageIds = entities.ToArray().Cast<OutMessage>().Select(m => m.EbmsMessageId).ToArray();
                _context.ReceptionAwareness.RemoveRange(
                    _context.ReceptionAwareness.Where(r => ebmsMessageIds.Contains(r.InternalMessageId)).ToArray());
            }

            _context.RemoveRange(entities);
            _context.SaveChanges();
        }
    }
}