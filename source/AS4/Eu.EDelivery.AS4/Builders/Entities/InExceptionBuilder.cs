using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Serialization;
using ReceivePMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;

namespace Eu.EDelivery.AS4.Builders.Entities
{
    /// <summary>
    /// Builder to create <see cref="InException"/> Models
    /// </summary>
    public class InExceptionBuilder
    {
        private AS4Exception _as4Exception;
        private string _messageId;

        /// <summary>
        /// Add a <see cref="AS4Exception"/> to the Builder
        /// </summary>
        /// <param name="as4Exception"></param>
        /// <returns></returns>
        public InExceptionBuilder WithAS4Exception(AS4Exception as4Exception)
        {
            this._as4Exception = as4Exception;
            return this;
        }

        /// <summary>
        /// Add a AS4 Message Id to the <see cref="InException"/>
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public InExceptionBuilder WithEbmsMessageId(string messageId)
        {
            this._messageId = messageId;
            return this;
        }

        /// <summary>
        /// Start creating an <see cref="InException"/> Model
        /// </summary>
        /// <returns></returns>
        public InException Build()
        {
            if (this._as4Exception == null)
                throw new AS4Exception("Builder needs an AS4Exception for building a InException");

            return new InException
            {
                EbmsRefToMessageId = this._messageId,
                Exception = this._as4Exception.ToString(),
                Operation = GetOperation(),
                ExceptionType = this._as4Exception.ExceptionType,
                PMode = this._as4Exception.PMode,

                // TODO: define Operation Method
                OperationMethod = "To be determined",
                ModificationTime = DateTimeOffset.UtcNow,
                InsertionTime = DateTimeOffset.UtcNow
            };
        }

        private Operation GetOperation()
        {
            if (string.IsNullOrEmpty(this._as4Exception.PMode))
                return Operation.NotApplicable;

            var pmode = AS4XmlSerializer.FromStream<ReceivePMode>(this._as4Exception.PMode);
            if (pmode == null) return Operation.NotApplicable;

            return pmode.ExceptionHandling.NotifyMessageConsumer
                ? Operation.ToBeNotified
                : Operation.NotApplicable;
        }

        public T GetPMode<T>(string pmodeString) where T : class
        {
            return AS4XmlSerializer.FromStream<T>(pmodeString);
        }
    }
}