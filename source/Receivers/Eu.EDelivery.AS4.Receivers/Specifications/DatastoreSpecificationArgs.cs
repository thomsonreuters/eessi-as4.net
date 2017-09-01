using System;

namespace Eu.EDelivery.AS4.Receivers.Specifications
{
    internal class DatastoreSpecificationArgs
    {
        public string TableName { get; }
        public string Filter { get; }
        public int TakeRecords { get; }

        public string DisplayString => $"FROM {TableName} WHERE {Filter} LIMIT {TakeRecords}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreSpecificationArgs"/> class.
        /// </summary>
        /// <param name="tableName">The table Name.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="take">The take.</param>
        public DatastoreSpecificationArgs(string tableName, string filter, int take = 20)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("A tablename should be specified.", nameof(tableName));
            }

            if (string.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentException("A column where to filter on should be specified.", nameof(filter));
            }

            TableName = tableName;
            Filter = filter;
            TakeRecords = take;
        }
    }
}