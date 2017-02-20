using System;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class ExceptionMessage
    {
        public string EbmsRefToMessageId { get; set; }
        public string Exception { get; set; }
        public string PMode { get; set; }
        public DateTimeOffset ModificationTime { get; set; }
        public DateTimeOffset InsertionTime { get; set; }
        public string Operation { get; set; }
    }
}