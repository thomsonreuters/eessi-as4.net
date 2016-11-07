using System;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// The reception awareness is responsible to keep track of the retry mechanism of then S-MSH
    /// </summary>
    public class ReceptionAwareness : Entity
    {
        public string InternalMessageId { get; set; }
        public int CurrentRetryCount { get; set; }
        public int TotalRetryCount { get; set; }
        public string RetryInterval { get; set; }
        public DateTimeOffset LastSendTime { get; set; }
        public bool IsCompleted { get; set; }
    }
}
