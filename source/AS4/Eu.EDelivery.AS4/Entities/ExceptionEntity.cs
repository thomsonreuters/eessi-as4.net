using System;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Entity for Exceptions
    /// </summary>
    public class ExceptionEntity : Entity
    {
        public string EbmsRefToMessageId { get; set; }
        public string Exception { get; set; }
        public string PMode { get; set; }
        public byte[] MessageBody { get; set; }

        public DateTimeOffset ModificationTime { get; set; }
        public DateTimeOffset InsertionTime { get; set; }
    }
}
