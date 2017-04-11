using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="OutException"/> Models
    /// </summary>
    public class OutExceptionBuilder
    {
        private readonly OutException _outException;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutExceptionBuilder"/> class. 
        /// Start Builder with default settings
        /// </summary>
        private OutExceptionBuilder()
        {
            _outException = new OutException { OperationMethod = "to be determined" };
        }

        public static OutExceptionBuilder ForAS4Exception(AS4Exception as4Exception)
        {
            var builder = new OutExceptionBuilder();
            return builder.WithAS4Exception(as4Exception);
        }

        /// <summary>
        /// Add a <see cref="AS4Exception"/>
        /// to the Builder
        /// </summary>
        /// <param name="as4Exception"></param>
        /// <returns></returns>
        private OutExceptionBuilder WithAS4Exception(AS4Exception as4Exception)
        {
            // Make sure that the Message of the most inner exception is available in the datastore.

            _outException.Exception = as4Exception.ToString();
            _outException.PMode = as4Exception.PMode;
            return this;
        }

        public OutExceptionBuilder WithEbmsMessageId(string messageId)
        {
            _outException.EbmsRefToMessageId = messageId;
            return this;
        }

        public OutExceptionBuilder WithOperation(Operation operation, string operationMethod = "To be determined")
        {
            _outException.Operation = operation;
            _outException.OperationMethod = operationMethod;
            return this;
        }

        /// <summary>
        /// Start creating a <see cref="OutException"/>
        /// </summary>
        /// <returns></returns>
        public OutException Build()
        {
            _outException.InsertionTime = DateTimeOffset.UtcNow;
            _outException.ModificationTime = DateTimeOffset.UtcNow;
            return _outException;
        }
    }
}
