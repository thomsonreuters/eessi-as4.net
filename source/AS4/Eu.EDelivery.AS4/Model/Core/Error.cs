using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Error : SignalMessage
    {
        /// <summary>
        /// Gets the <see cref="ErrorLine"/>'s which for which this <see cref="Error"/> is created.
        /// </summary>
        public IEnumerable<ErrorLine> ErrorLines { get; } = Enumerable.Empty<ErrorLine>();

        /// <summary>
        /// Gets the multihop action value.
        /// </summary>
        public override string MultihopAction { get; } = Constants.Namespaces.EbmsOneWayError;

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        public Error(string refToMessageId) 
            : base(IdentifierFactory.Instance.Create(), refToMessageId) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="detail"></param>
        public Error(string refToMessageId, ErrorLine detail) 
            : this(IdentifierFactory.Instance.Create(), refToMessageId, detail) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="routing"></param>
        public Error(string refToMessageId, RoutingInputUserMessage routing)
            : base(IdentifierFactory.Instance.Create(), refToMessageId, routing) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="detail"></param>
        /// <param name="routing"></param>
        public Error(string refToMessageId, ErrorLine detail, RoutingInputUserMessage routing)
            : base(IdentifierFactory.Instance.Create(), refToMessageId, routing)
        {
            if (detail == null)
            {
                throw new ArgumentNullException(nameof(detail));
            }

            ErrorLines = new[] { detail };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        public Error(string messageId, string refToMessageId) : base(messageId, refToMessageId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="line"></param>
        public Error(string messageId, string refToMessageId, ErrorLine line) : base(messageId, refToMessageId)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            ErrorLines = new[] { line };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="lines"></param>
        public Error(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            IEnumerable<ErrorLine> lines)
            : base(messageId, refToMessageId, timestamp)
        {
            if (lines == null || lines.Any(d => d is null))
            {
                throw new ArgumentNullException(nameof(lines));
            }

            ErrorLines = lines;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="lines"></param>
        /// <param name="routing"></param>
        public Error(
            string messageId, 
            string refToMessageId, 
            DateTimeOffset timestamp, 
            IEnumerable<ErrorLine> lines, 
            RoutingInputUserMessage routing) : base(messageId, refToMessageId, timestamp, routing)
        {
            if (lines == null || lines.Any(l => l is null))
            {
                throw new ArgumentNullException(nameof(lines));
            }

            ErrorLines = lines;
        }

        /// <summary>
        /// Creates a new <see cref="Error"/> model from an <see cref="ErrorResult"/> instance.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Error FromErrorResult(string messageId, string refToMessageId, ErrorResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new Error(messageId, refToMessageId, ErrorLine.FromErrorResult(result));
        }

        /// <summary>
        /// Creates a new <see cref="Error"/> model from an <see cref="ErrorResult"/> instance.
        /// </summary>
        /// <param name="refToMessageId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Error FromErrorResult(string refToMessageId, ErrorResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new Error(refToMessageId, ErrorLine.FromErrorResult(result));
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Error"/> is originated from a Pull Request.
        /// </summary>
        [XmlIgnore]
        public bool IsWarningForEmptyPullRequest
        {
            get
            {
                ErrorLine firstPullRequestError =
                    ErrorLines.FirstOrDefault(
                        detail =>
                            detail.Severity == Severity.WARNING
                            && detail.ShortDescription == ErrorAlias.EmptyMessagePartitionChannel);

                return firstPullRequestError != null;
            }
        }
    }

    public class ErrorLine
    {
        public ErrorCode ErrorCode { get; }

        public Severity Severity { get; }

        public Maybe<string> Origin { get; }

        public Maybe<string> Category { get; }

        public Maybe<string> RefToMessageInError { get; }

        public ErrorAlias ShortDescription { get; }

        public Maybe<ErrorDescription> Description { get; }

        public Maybe<string> Detail { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLine"/> class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="severity"></param>
        /// <param name="shortDescription"></param>
        public ErrorLine(
            ErrorCode errorCode,
            Severity severity,
            ErrorAlias shortDescription)
        {
            ErrorCode = errorCode;
            Severity = severity;
            Origin = Maybe<string>.Nothing;
            Category = Maybe<string>.Nothing;
            RefToMessageInError = Maybe<string>.Nothing;
            ShortDescription = shortDescription;
            Description = Maybe<ErrorDescription>.Nothing;
            Detail = Maybe<string>.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLine"/> class.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="severity"></param>
        /// <param name="shortDescription"></param>
        /// <param name="origin"></param>
        /// <param name="category"></param>
        /// <param name="refToMessageInError"></param>
        /// <param name="description"></param>
        /// <param name="detail"></param>
        public ErrorLine(
            ErrorCode errorCode,
            Severity severity,
            ErrorAlias shortDescription,
            Maybe<string> origin,
            Maybe<string> category,
            Maybe<string> refToMessageInError,
            Maybe<ErrorDescription> description,
            Maybe<string> detail)
        {
            if (origin == null)
            {
                throw new ArgumentNullException(nameof(origin));
            }

            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (refToMessageInError == null)
            {
                throw new ArgumentNullException(nameof(refToMessageInError));
            }

            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            if (detail == null)
            {
                throw new ArgumentNullException(nameof(detail));
            }

            ErrorCode = errorCode;
            Severity = severity;
            Origin = origin;
            Category = category;
            RefToMessageInError = refToMessageInError;
            ShortDescription = shortDescription;
            Description = description;
            Detail = detail;
        }

        private ErrorLine(
            ErrorCode errorCode, 
            Severity severity, 
            Maybe<string> category, 
            ErrorAlias shortDescription,  
            string detail)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (detail == null)
            {
                throw new ArgumentNullException(nameof(detail));
            }

            ErrorCode = errorCode;
            Severity = severity;
            Origin = Maybe<string>.Nothing;
            Category = category;
            RefToMessageInError = Maybe<string>.Nothing;
            ShortDescription = shortDescription;
            Description = Maybe<ErrorDescription>.Nothing;
            Detail = Maybe.Just(detail);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ErrorLine"/> class from a <see cref="ErrorResult"/> instance.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static ErrorLine FromErrorResult(ErrorResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            string category = ErrorCodeUtils.GetCategory(result.Code);
            return new ErrorLine(
                result.Code,
                Severity.FAILURE,
                (category != null).ThenMaybe(category),
                result.Alias,
                result.Description);
        }
    }

    public class ErrorDescription
    {
        public string Language { get; }

        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDescription"/> class.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="value"></param>
        public ErrorDescription(string language, string value)
        {
            if (language == null)
            {
                throw new ArgumentNullException(nameof(language));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Language = language;
            Value = value;
        }
    }

    public enum Severity
    {
        FAILURE,
        WARNING
    }
}