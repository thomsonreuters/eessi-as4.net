using System;
using System.Linq;

namespace Eu.EDelivery.AS4.Fe.Monitor.Model
{
    public class ExceptionMessage
    {
        private string exception;
        public Direction Direction { get; set; }
        public string EbmsRefToMessageId { get; set; }
        public string Exception
        {
            get { return exception; }
            set
            {
                exception = value;
                // Remove the message id from the exception
                ExceptionShort = !string.IsNullOrEmpty(value) ? value.Substring(value.LastIndexOf(']') + 1).Split('\r', '\n').FirstOrDefault() : string.Empty;
            }
        }
        public string ExceptionShort { get; set; }
        public string PMode { get; set; }
        public DateTimeOffset ModificationTime { get; set; }
        public DateTimeOffset InsertionTime { get; set; }
        public string Operation { get; set; }
        public string Hash { get; set; }
    }
}