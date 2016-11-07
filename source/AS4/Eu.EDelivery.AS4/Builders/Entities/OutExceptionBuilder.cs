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
        public OutExceptionBuilder()
        {
            this._outException = new OutException {OperationMethod = "to be determined"};
        }

        /// <summary>
        /// Add a <see cref="AS4Exception"/>
        /// to the Builder
        /// </summary>
        /// <param name="as4Exception"></param>
        /// <returns></returns>
        public OutExceptionBuilder WithAS4Exception(AS4Exception as4Exception)
        {
            this._outException.Exception = as4Exception.ToString();
            this._outException.PMode = as4Exception.PMode;
            return this;
        }

        public OutExceptionBuilder WithEbmsMessageId(string messageId)
        {
            this._outException.EbmsRefToMessageId = messageId;
            return this;
        }

        public OutExceptionBuilder WithOperation(Operation operation, string operationMethod = "To be determined")
        {
            this._outException.Operation = operation;
            this._outException.OperationMethod = operationMethod;
            return this;
        }

        /// <summary>
        /// Start creating a <see cref="OutException"/>
        /// </summary>
        /// <returns></returns>
        public OutException Build()
        {
            this._outException.InsertionTime = DateTimeOffset.UtcNow;
            this._outException.ModificationTime = DateTimeOffset.UtcNow;
            return this._outException;
        }
    }
}
