using System;
using Eu.EDelivery.AS4.Fe.Hash;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class Message
    {
        public string Operation { get; set; }
        public string OperationMethod { get; set; }
        public string ContentType { get; set; }
        public string EbmsMessageId { get; set; }
        public string EbmsMessageType { get; set; }
        public string EbmsRefToMessageId { get; set; }
        public DateTimeOffset ModificationTime { get; set; }
        public DateTimeOffset InsertionTime { get; set; }
        public string ExceptionType { get; set; }
        public string Status { get; set; }
        public string PMode { get; set; }
        public bool HasExceptions { get; set; }
        public string Hash => this.GetMd5Hash();
    }
}