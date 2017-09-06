using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Incoming Message Data Entity Schema
    /// </summary>
    public class InMessage : MessageEntity
    {        
        public void SetStatus(InStatus status)
        {
            Status = status.ToString();
        }
    }
}