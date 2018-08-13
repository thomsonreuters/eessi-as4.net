using System;
using System.ComponentModel.DataAnnotations;
using Eu.EDelivery.AS4.Agents;

namespace Eu.EDelivery.AS4.Entities
{
    public class Journal : Entity
    {
        public long? RefToInMessageId { set; get; }

        public long? RefToOutMessageId { get; set; }

        public DateTimeOffset LogDate { get; set; }

        [MaxLength(20)]
        public string AgentType { get; private set; }

        public void SetAgentType(AgentType t)
        {
            AgentType = t.ToString();
        }

        [MaxLength(50)]
        public string AgentName { get; set; }

        [MaxLength(100)]
        public string EbmsMessageId { get; set; }

        public string LogEntry { get; set; }

        [MaxLength(20)]
        public string MessageStatus { get; private set; }

        public void SetStatus(InStatus s)
        {
            MessageStatus = s.ToString();
        }

        public void SetStatus(OutStatus s)
        {
            MessageStatus = s.ToString();
        }

        [MaxLength(20)]
        public string MessageOperation { get; private set; }

        public void SetOperation(Operation o)
        {
            MessageOperation = o.ToString();
        }
    }
}
