using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class SqlServerDbCommand : IAS4DbCommand
    {
        private readonly IQueryable<Entity> _dbSet;
        private readonly DatastoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDbCommand" /> class.
        /// </summary>
        /// <param name="dbSet">The database set.</param>
        /// <param name="context">The context.</param>
        public SqlServerDbCommand(IQueryable<Entity> dbSet, DatastoreContext context)
        {
            _dbSet = dbSet;
            _context = context;
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
            string query =
                $@"SELECT TOP {takeRows} *
                FROM {tableName} WITH (XLOCK, READPAST)
                WHERE {filter}
                ORDER BY InsertionTime";

            IQueryable<Entity> entities = _dbSet.FromSql(query);

            return entities.AsEnumerable();
        }
    }
}
