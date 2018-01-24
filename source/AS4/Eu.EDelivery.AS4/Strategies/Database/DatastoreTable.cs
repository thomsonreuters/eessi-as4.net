using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    /// <summary>
    /// Validate the name of the Datastore Table.
    /// </summary>
    public class DatastoreTable
    {
        private static readonly ConcurrentDictionary<string, bool> KnownTables = new ConcurrentDictionary<string, bool>();

        private static readonly IDictionary<string, Func<DatastoreContext, IQueryable<Entity>>> _tablesByName =
            new Dictionary<string, Func<DatastoreContext, IQueryable<Entity>>>
            {
                {"InMessages", c => c.InMessages},
                {"OutMessages", c => c.OutMessages},
                {"InExceptions", c => c.InExceptions},
                {"OutExceptions", c => c.OutExceptions},
                {"ReceptionAwareness", c => c.ReceptionAwareness}
            };

        /// <summary>
        /// Determines whether [is table name known] [the specified table name].
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>
        ///   <c>true</c> if [is table name known] [the specified table name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTableNameKnown(string tableName)
        {
            return KnownTables.GetOrAdd(tableName, t => typeof(DatastoreContext).GetProperty(t) != null);
        }

        public static Func<DatastoreContext, IQueryable<Entity>> FromTableName(string tableName)
        {
            if (!_tablesByName.ContainsKey(tableName))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }

            return _tablesByName[tableName];
        }
    }
}
