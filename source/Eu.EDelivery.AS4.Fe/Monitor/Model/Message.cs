using System;
using Eu.EDelivery.AS4.Fe.Hash;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    /// <summary>
    /// Class to hold message data
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public string Operation { get; set; }
        /// <summary>
        /// Gets or sets the operation method.
        /// </summary>
        /// <value>
        /// The operation method.
        /// </value>
        public string OperationMethod { get; set; }
        /// <summary>
        /// Gets or sets the ebms message identifier.
        /// </summary>
        /// <value>
        /// The ebms message identifier.
        /// </value>
        public string EbmsMessageId { get; set; }
        /// <summary>
        /// Gets or sets the type of the ebms message.
        /// </summary>
        /// <value>
        /// The type of the ebms message.
        /// </value>
        public string EbmsMessageType { get; set; }
        /// <summary>
        /// Gets or sets the ebms reference to message identifier.
        /// </summary>
        /// <value>
        /// The ebms reference to message identifier.
        /// </value>
        public string EbmsRefToMessageId { get; set; }
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
        /// Gets or sets the type of the exception.
        /// </summary>
        /// <value>
        /// The type of the exception.
        /// </value>
        public string ExceptionType { get; set; }
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public string Status { get; set; }
        /// <summary>
        /// Gets or sets from party.
        /// </summary>
        /// <value>
        /// From party.
        /// </value>
        public string FromParty { get; set; }
        /// <summary>
        /// Gets or sets to party.
        /// </summary>
        /// <value>
        /// To party.
        /// </value>
        public string ToParty { get; set; }
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }
        /// <summary>
        /// Gets or sets the MPC.
        /// </summary>
        /// <value>
        /// The MPC.
        /// </value>
        public string Mpc { get; set; }
        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// <value>
        /// The direction.
        /// </value>
        public Direction Direction { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is duplicate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is duplicate; otherwise, <c>false</c>.
        /// </value>
        public bool IsDuplicate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is test.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is test; otherwise, <c>false</c>.
        /// </value>
        public bool IsTest { get; set; }
        /// <summary>
        /// Gets or sets the mep.
        /// </summary>
        /// <value>
        /// The mep.
        /// </value>
        public string Mep { get; set; }
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the p mode.
        /// </summary>
        /// <value>
        /// The p mode.
        /// </value>
        public string PMode
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && value.ToLower().Contains("xml"))
                {
                    Hash = value.GetMd5Hash();
                }
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
        /// Gets or sets a value indicating whether this instance has exceptions.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has exceptions; otherwise, <c>false</c>.
        /// </value>
        public bool HasExceptions { get; set; }
        /// <summary>
        /// Gets or sets the hash.
        /// </summary>
        /// <value>
        /// The hash.
        /// </value>
        public string Hash { get; set; }
    }
}