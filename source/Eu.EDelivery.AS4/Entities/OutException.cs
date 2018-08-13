using System;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Outcoming Message Exception Data Entity Schema
    /// </summary>
    public class OutException : ExceptionEntity
    {
        // ReSharper disable once UnusedMember.Local - default ctor is required for EntityFramework.
        private OutException() { }

        public OutException(string ebmsRefToMessageId, Exception exception)
            : base(ebmsRefToMessageId, exception) { }

        public OutException(string ebmsRefToMessageId, string errorMessage)
            : base(ebmsRefToMessageId, errorMessage) { }

        public OutException(byte[] messageBody, Exception exception)
            : base(messageBody, exception) { }

        public OutException(byte[] messageBody, string errorMessage)
            : base(messageBody, errorMessage)
        { }
    }
}