using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    public class DbCommandProvider
    {
        private readonly IDictionary<string, Func<IQueryable<Entity>, DatastoreContext, IAS4DbCommand>> _commands;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommandProvider"/> class.
        /// </summary>
        public DbCommandProvider()
        {
            _commands =
                new Dictionary<string, Func<IQueryable<Entity>, DatastoreContext, IAS4DbCommand>>(
                    StringComparer.InvariantCultureIgnoreCase)
                {
                    {"SqlServer", (db, ctx) => new SqlServerDbCommand(db, ctx)},
                    {"Sqlite", (db, ctx) => new SqliteDbCommand(db, ctx)}
                };
        }

        /// <summary>
        /// Gets a <see cref="IAS4DbCommand"/> implementation based on the specified DBMS-type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public Func<IQueryable<Entity>, DatastoreContext, IAS4DbCommand> Get(string type)
        {
            if (_commands.TryGetValue(type, out Func<IQueryable<Entity>, DatastoreContext, IAS4DbCommand> creation))
            {
                return creation;
            }

            throw new KeyNotFoundException($"No Database Command found for DBMS-Type: '{type}'");
        }
    }
}
