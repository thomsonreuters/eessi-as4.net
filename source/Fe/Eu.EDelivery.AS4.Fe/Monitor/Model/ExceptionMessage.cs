using System;
using System.Linq;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    /// <summary>
    /// Class to contain exception messages
    /// </summary>
    public class ExceptionMessage
    {
        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        public Direction Direction { get; set; }
        /// <summary>
        /// Gets or sets the ebms reference to message identifier.
        /// </summary>
        /// <value>
        /// The ebms reference to message identifier.
        /// </value>
        public string EbmsRefToMessageId { get; set; }
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public string Exception { get; set; }
        /// <summary>
        /// Gets or sets the exception short.
        /// </summary>
        /// <value>
        /// The exception short.
        /// </value>
        public string ExceptionShort { get; set; }
        /// <summary>
        /// Gets or sets the p mode.
        /// </summary>
        /// <value>
        /// The p mode.
        /// </value>
        public string PMode { get; set; }
        /// <summary>
        /// Gets or sets the modification time.
        /// </summary>
        /// <value>
        /// The modification time.
        /// </value>
        public DateTimeOffset ModificationTime { get; set; }
        /// <summary>
        /// Gets or sets the insertion time.
        /// </summary>
        /// <value>
        /// The insertion time.
        /// </value>
        public DateTimeOffset InsertionTime { get; set; }
        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public string Operation { get; set; }
        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        /// <value>
        /// The hash.
        /// </value>
        public string Hash { get; set; }
    }
}