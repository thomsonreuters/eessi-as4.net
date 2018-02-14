using System;
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
    public static class DatastoreTable
    {
        private static readonly IDictionary<string, Func<DatastoreContext, IQueryable<Entity>>> TablesByName =
            new Dictionary<string, Func<DatastoreContext, IQueryable<Entity>>>
            {
                {"InMessages", c => c.InMessages},
                {"OutMessages", c => c.OutMessages},
                {"InExceptions", c => c.InExceptions},
                {"OutExceptions", c => c.OutExceptions},
                {"ReceptionAwareness", c => c.ReceptionAwareness}
            };

        /// <summary>
        /// Gets the message tables.
        /// </summary>
        /// <value>The message tables.</value>
        public static IEnumerable<string> MessageTables => TablesByName.Keys.Where(k => !k.Equals("ReceptionAwareness"));

        /// <summary>
        /// Determines whether [is table name known] [the specified table name].
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>
        ///   <c>true</c> if [is table name known] [the specified table name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTableNameKnown(string tableName)
        {
            return TablesByName.ContainsKey(tableName);
        }

        public static Func<DatastoreContext, IQueryable<Entity>> FromTableName(string tableName)
        {
            if (!TablesByName.ContainsKey(tableName))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }

            return TablesByName[tableName];
        }
    }
}
