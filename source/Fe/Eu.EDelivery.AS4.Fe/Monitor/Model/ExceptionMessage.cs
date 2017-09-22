using System;
using Eu.EDelivery.AS4.Fe.Hash;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    /// <summary>
    /// Class to contain exception messages
    /// </summary>
    public class ExceptionMessage
    {
        private string pMode;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long Id { get; set; }
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
        public string PMode
        {
            get { return pMode; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.ToLower().Contains("xml"))
                {
                    Hash = value.GetMd5Hash();
                }
                pMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the pmode identifier.
        /// </summary>
        /// <value>
        /// The pmode identifier.
        /// </value>
        public string PModeId { get; set; }

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
        /// <summary>
        /// Gets or sets a value indicating whether this instance has a message body.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has a message body; otherwise, <c>false</c>.
        /// </value>
        public bool HasMessageBody { get; set; }
    }
}