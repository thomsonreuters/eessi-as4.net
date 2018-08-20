using System;

namespace Eu.EDelivery.AS4.Receivers
{
    public sealed class DatastoreReceiverSettings
    {
        public string TableName { get; }
        public string Filter { get; }
        public string UpdateField { get; }
        public string UpdateValue { get; }
        public TimeSpan PollingInterval { get; }
        public int TakeRows { get; }

        public const int DefaultTakeRows = 20;
        public static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromSeconds(3);

        public string DisplayString => $"FROM {TableName} WHERE {Filter} LIMIT {TakeRows}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiverSettings"/> class.
        /// </summary>
        public DatastoreReceiverSettings(string tableName, string filter, string updateField, string updateValue):
            this(tableName, filter, updateField, updateValue, DefaultPollingInterval, DefaultTakeRows)
        {                
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatastoreReceiverSettings"/> class.
        /// </summary>
        public DatastoreReceiverSettings(string tableName, string filter, string updateField, string updateValue, TimeSpan pollingInterval, int takeRows = DefaultTakeRows)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("A tablename must be specified.", nameof(tableName));
            }
            if (String.IsNullOrWhiteSpace(filter))
            {
                throw new ArgumentException("A filter expression must be specified.", nameof(filter));
            }
            if (String.IsNullOrWhiteSpace(updateField))
            {
                throw new ArgumentException("An updatefield must be specified.", nameof(updateField));
            }
            if (String.IsNullOrWhiteSpace(updateValue))
            {
                throw new ArgumentException("An updatevalue must be specified.", nameof(updateValue));
            }

            TableName = tableName;
            Filter = filter;
            UpdateField = updateField;
            UpdateValue = updateValue;
            TakeRows = takeRows;
            PollingInterval = pollingInterval;
        }
    }
}
