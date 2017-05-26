using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Error = Eu.EDelivery.AS4.Model.Core.Error;

namespace Eu.EDelivery.AS4.Builders.Core
{
    /// <summary>
    /// Builder to create <see cref="Model.Core.Error"/> Models
    /// </summary>
    public class ErrorBuilder
    {
        private readonly Error _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBuilder"/> class. 
        /// Start Builder with default settings
        /// </summary>
        public ErrorBuilder()
        {
            _errorMessage = new Error();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBuilder"/> class. 
        /// Start Builder with a given AS4 Message Id
        /// </summary>
        /// <param name="messageId">
        /// </param>
        public ErrorBuilder(string messageId)
        {
            _errorMessage = new Error(messageId);
        }

        /// <summary>
        /// Add a AS4 Message Id to the <see cref="Error"/> Model
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public ErrorBuilder WithRefToEbmsMessageId(string messageId)
        {
            _errorMessage.RefToMessageId = messageId;

            return this;
        }

        /// <summary>
        /// Add a <see cref="AS4Exception"/> details
        /// to the <see cref="Error"/> Message
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public ErrorBuilder WithAS4Exception(AS4Exception exception)
        {
            _errorMessage.Exception = exception;
            _errorMessage.Errors = CreateErrorDetails(exception);

            return this;
        }

        public ErrorBuilder WithOriginalMessage(InternalMessage message)
        {
            if (message.SendingPMode?.MessagePackaging.IsMultiHop == true)
            {
                _errorMessage.MultiHopRouting = AS4Mapper.Map<RoutingInputUserMessage>(message.AS4Message.PrimaryUserMessage);
            }

            return this;
        }

        private static IList<ErrorDetail> CreateErrorDetails(AS4Exception exception)
        {
            var errorDetails = new List<ErrorDetail>();
            foreach (string messageId in exception.MessageIds)
            {
                ErrorDetail detail = CreateErrorDetail(exception);
                detail.RefToMessageInError = messageId;
                errorDetails.Add(detail);
            }

            return errorDetails;
        }

        private static ErrorDetail CreateErrorDetail(AS4Exception exception)
        {
            return new ErrorDetail
            {
                Detail = exception.Message,
                Severity = Severity.FAILURE,
                ErrorCode = $"EBMS:{(int)exception.ErrorCode:0000}",
                Category = ErrorCodeUtils.GetCategory(exception.ErrorCode),
                ShortDescription = ErrorCodeUtils.GetShortDescription(exception.ErrorCode)
            };
        }

        /// <summary>
        /// Build the <see cref="Error"/> Model
        /// </summary>
        /// <returns></returns>
        public Error Build()
        {
            if (!string.IsNullOrEmpty(_errorMessage.MessageId))
            {
                _errorMessage.Exception?.AddMessageId(_errorMessage.MessageId);
            }

            _errorMessage.Timestamp = DateTimeOffset.UtcNow;
            return _errorMessage;
        }

        public Error BuildWithOriginalAS4Exception()
        {
            _errorMessage.Timestamp = DateTimeOffset.UtcNow;
            return _errorMessage;
        }
    }
}