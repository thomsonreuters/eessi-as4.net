using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class BaseMessageJoined<T>
        where T : MessageEntity
    {
        public T Message { get; set; }
        public bool HasExceptions { get; set; }
    }
}