using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class MessageResult<T>
    {
        public IEnumerable<T> Messages { get; set; }
        public int Total { get; set; }
        public int CurrentPage { get; set; } = 0;
        public int Pages { get; set; }
        public int Page { get; set; }
    }
}