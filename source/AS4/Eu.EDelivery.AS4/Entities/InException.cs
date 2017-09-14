using System;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// Incoming Message Exception Data Entity Schema
    /// </summary>
    public class InException : ExceptionEntity
    {
        // ReSharper disable once UnusedMember.Local - default ctor is required for Entity Framework.
        private InException() { }

        public InException(string ebmsRefToMessageId, Exception exception)
            : base(ebmsRefToMessageId, exception) { }

        public InException(string ebmsRefToMessageId, string errorMessage)
            : base(ebmsRefToMessageId, errorMessage)
        { }

        public InException(byte[] messageBody, Exception exception)
            : base(messageBody, exception) { }

        public InException(byte[] messageBody, string errorMessage)
            : base(messageBody, errorMessage) { }
    }
}