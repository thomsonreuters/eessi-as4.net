using System.Collections.Concurrent;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    /// <summary>
    /// Validate the name of the Datastore Table.
    /// </summary>
    public class TableValidation
    {
        private static readonly ConcurrentDictionary<string, bool> KnownTables = new ConcurrentDictionary<string, bool>();

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
    }
}
