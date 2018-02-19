﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Strategies.Database
{
    internal class SqliteDbCommand : IAS4DbCommand
    {
        private readonly DatastoreContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteDbCommand" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SqliteDbCommand(DatastoreContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialization process for the different DBMS storage types.
        /// </summary>
        public async Task CreateDatabase()
        {
            await _context.Database.MigrateAsync();
        }

        /// <summary>
        /// Wraps the given <paramref name="funcToWrap"/> into a DBMS storage type specific transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcToWrap">The function to wrap.</param>
        /// <returns></returns>
        public T WithTransaction<T>(Func<DatastoreContext, T> funcToWrap)
        {
            _context.Database.ExecuteSqlCommand("BEGIN EXCLUSIVE");
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
            if (!DatastoreTable.IsTableNameKnown(tableName))
            {
                throw new ConfigurationErrorsException($"The configured table {tableName} could not be found");
            }

            string filterExpression = filter.Replace("\'", "\"");

            return DatastoreTable.FromTableName(tableName)(_context)
                .Where(filterExpression)
                .OrderBy(x => x.InsertionTime)
                .Take(takeRows)
                .ToList();
        }
    }
}
