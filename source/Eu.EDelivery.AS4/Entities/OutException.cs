using System;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Outcoming Message Exception Data Entity Schema
    /// </summary>
    public class OutException : ExceptionEntity
    {
        // ReSharper disable once UnusedMember.Local - default ctor is required for EntityFramework.
        private OutException() { }

        private OutException(
            string ebmsRefToMessageId,
            string messageLocation,
            Exception exception) : base(ebmsRefToMessageId, messageLocation, exception) { }

        private OutException(
            string ebmsRefToMessageId,
            string messageLocation,
            string exception) : base(ebmsRefToMessageId, messageLocation, exception) { }

        /// <summary>
        /// Sets the <see cref="OutException.Operation"/> based on the configuration in the specified <paramref name="exceptionHandling"/>.
        /// </summary>
        /// <param name="exceptionHandling">The exception handling of the <see cref="SendingProcessingMode"/></param>
        public OutException SetOperationFor(SendHandling exceptionHandling)
        {
            bool needsToBeNotified = exceptionHandling?.NotifyMessageProducer == true;
            Operation = needsToBeNotified ? Operation.ToBeNotified : default(Operation);
            return this;
        }

        /// <summary>
        /// Creates an <see cref="OutException"/> that references an exsiting stored message.
        /// </summary>
        /// <param name="ebmsRefToMessageId">The message id of the message that caused the exception.</param>
        /// <param name="exception">The occurred exception for which we have to insert a record.</param>
        public static OutException ForEbmsMessageId(string ebmsRefToMessageId, Exception exception)
        {
            return new OutException(ebmsRefToMessageId: ebmsRefToMessageId, messageLocation: null, exception: exception);
        }

        /// <summary>
        /// Creates an <see cref="OutException"/> that references an exsiting stored message.
        /// </summary>
        /// <param name="ebmsRefToMessageId">The message id of the message that caused the exception.</param>
        /// <param name="exception">The occurred exception message for which we have to insert a record.</param>
        public static OutException ForEbmsMessageId(string ebmsRefToMessageId, string exception)
        {
            return new OutException(ebmsRefToMessageId: ebmsRefToMessageId, messageLocation: null, exception: exception);
        }

        /// <summary>
        /// Creates an <see cref="InException"/> that uses a stored location of the original message because it can't be referenced to an existing stored message.
        /// </summary>
        /// <param name="messageLocation">The location to where the refering message which caused the exception is stored.</param>
        /// <param name="exception">The occurred exception for which we have to insert a record.</param>
        public static OutException ForMessageBody(string messageLocation, Exception exception)
        {
            return new OutException(ebmsRefToMessageId: null, messageLocation: messageLocation, exception: exception);
        }

        /// <summary>
        /// Creates an <see cref="InException"/> that uses a stored location of the original message because it can't be referenced to an existing stored message.
        /// </summary>
        /// <param name="messageLocation">The location to where the refering message which caused the exception is stored.</param>
        /// <param name="exception">The occurred exception message for which we have to insert a record.</param>
        public static OutException ForMessageBody(string messageLocation, string exception)
        {
            return new OutException(ebmsRefToMessageId: null, messageLocation: messageLocation, exception: exception);
        }
    }
}