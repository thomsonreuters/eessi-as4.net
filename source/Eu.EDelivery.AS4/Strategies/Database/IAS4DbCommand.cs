using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    /// <summary>
    /// Abstraction to introduce custom commands/queries on the specific DBMS storage types.
    /// </summary>
    public interface IAS4DbCommand
    {
        /// <summary>
        /// Gets the exclusive lock isolation for the transaction of retrieval of entities.
        /// </summary>
        /// <value>The exclusive lock isolation.</value>
        IsolationLevel? ExclusiveLockIsolation { get; }

        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        Task CreateDatabase();

        /// <summary>
        /// Exclusively retrieves the entities for the different DBMS storage types.
        /// </summary>
        /// <param name="tableName">Name of the Db table.</param>
        /// <param name="filter">Order by this field.</param>
        /// <param name="takeRows">Take this amount of rows.</param>
        /// <returns></returns>
        IEnumerable<Entity> ExclusivelyRetrieveEntities(
            string tableName,
            string filter,
            int takeRows);

        /// <summary>
        /// Delete the Messages Entities that are inserted passed a given <paramref name="retentionPeriod"/> 
        /// and has a <see cref="Operation"/> within the given <paramref name="allowedOperations"/>.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="retentionPeriod">The retention period.</param>
        /// <param name="allowedOperations">The allowed operations.</param>
        void BatchDeleteOverRetentionPeriod(
            string tableName,
            TimeSpan retentionPeriod,
            IEnumerable<Operation> allowedOperations);

        /// <summary>
        /// Selects in a reliable way the ToBePiggyBacked SignalMessages stored in the OutMessage table.
        /// </summary>
        /// <param name="url">The endpoint to which the OutMessage SignalMessage should be Piggy Backed.</param>
        /// <param name="mpc">The MPC of the incoming PullRequest to match on the related UserMessage of the Piggy Backed SignalMessage.</param>
        /// <returns></returns>
        IEnumerable<OutMessage> SelectToBePiggyBackedSignalMessages(string url, string mpc);
    }
}
