using Eu.EDelivery.AS4.Entities;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    public class BaseExceptionJoined<T>
        where T : ExceptionEntity
    {
        public T Message { get; set; }
    }
}