using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// ebMS signal message unit representing a failure response to an ebMS <see cref="UserMessage"/>.
    /// </summary>
    public class Error : SignalMessage
    {
        /// <summary>
        /// Gets the <see cref="ErrorLine"/>'s which for which this <see cref="Error"/> is created.
        /// </summary>
        public IEnumerable<ErrorLine> ErrorLines { get; } = Enumerable.Empty<ErrorLine>();

        /// <summary>
        /// Gets the multi-hop action value.
        /// </summary>
        public override string MultihopAction { get; } = Constants.Namespaces.EbmsOneWayError;

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        public Error(string messageId, string refToMessageId) : base(messageId, refToMessageId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="line">The single error entry to include in this error.</param>
        public Error(string messageId, string refToMessageId, ErrorLine line) 
            : this(messageId, refToMessageId, new [] { line }) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="lines">The sequence of error entries to include in this error.</param>
        public Error(string messageId, string refToMessageId, IEnumerable<ErrorLine> lines)
            : this(messageId, refToMessageId, DateTimeOffset.Now, lines) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="timestamp">The timestamp when this receipt is created.</param>
        /// <param name="lines">The sequence of error entries to include in this error.</param>
        internal Error(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            IEnumerable<ErrorLine> lines)
            : this(messageId, refToMessageId, timestamp, lines, routedUserMessage: null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="timestamp">The timestamp when this receipt is created.</param>
        /// <param name="lines">The sequence of error entries to include in this error.</param>
        /// <param name="routedUserMessage">The <see cref="Core.UserMessage"/> to include in the error in the form of a RoutingInput element.</param>
        internal Error(
            string messageId, 
            string refToMessageId, 
            DateTimeOffset timestamp, 
            IEnumerable<ErrorLine> lines, 
            RoutingInputUserMessage routedUserMessage) : base(messageId, refToMessageId, timestamp, routedUserMessage)
        {
            if (lines == null || lines.Any(l => l is null))
            {
                throw new ArgumentNullException(nameof(lines));
            }

            ErrorLines = lines;
        }

        /// <summary>
        /// Format the error lines of the AS4 Error into readable descriptions for each line.
        /// </summary>
        public string FormatErrorLines()
        {
            return String.Join(
                "; ",
                ErrorLines.Select(l => $"{l.ErrorCode.GetString()} {l.ShortDescription} {l.Detail.GetOrElse(String.Empty)}"));
        }

        /// <summary>
        /// Creates an AS4 error referencing a given <paramref name="userMessage"/>.
        /// </summary>
        /// <param name="errorMessageId">The ebMS message identifier of this message unit.</param>
        /// <param name="userMessage">The AS4 user message to reference in the to be created error.</param>
        /// <param name="userMessageSendViaMultiHop">
        ///     Whether or not the user message was send in a multi-hop fashion or not.
        ///     Setting this on <c>true</c> will result in an error with the referencing user message included in a RoutingInput element.
        /// </param>
        public static Error CreateFor(
            string errorMessageId, 
            UserMessage userMessage, 
            bool userMessageSendViaMultiHop = false)
        {
            return CreateFor(
                errorMessageId,
                userMessage, 
                occurredError: null, 
                userMessageSendViaMultiHop: userMessageSendViaMultiHop);
        }

        /// <summary>
        /// Creates an AS4 error referencing a given <paramref name="userMessage"/>.
        /// </summary>
        /// <param name="errorMessageId">The ebMS message identifier of this message unit.</param>
        /// <param name="userMessage">The AS4 user message to reference in the to be created error.</param>
        /// <param name="occurredError">The error that has happened during the step execution.</param>
        /// <param name="userMessageSendViaMultiHop">
        ///     Whether or not the user message was send in a multi-hop fashion or not.
        ///     Setting this on <c>true</c> will result in an error with the referencing user message included in a RoutingInput element.
        /// </param>
        public static Error CreateFor(
            string errorMessageId,
            UserMessage userMessage, 
            ErrorResult occurredError,
            bool userMessageSendViaMultiHop = false)
        {
            if (userMessageSendViaMultiHop)
            {
                var routedUserMessage = UserMessageMap.ConvertToRouting(userMessage);
                return occurredError == null
                    ? new Error(errorMessageId, userMessage?.MessageId, DateTimeOffset.Now, new ErrorLine[0], routedUserMessage)
                    : new Error(errorMessageId, userMessage?.MessageId, DateTimeOffset.Now, new [] { ErrorLine.FromErrorResult(occurredError) }, routedUserMessage);
            }

            return occurredError == null
                ? new Error(errorMessageId, userMessage?.MessageId)
                : new Error(errorMessageId, userMessage?.MessageId, ErrorLine.FromErrorResult(occurredError));
        }

        /// <summary>
        /// Creates an AS4 error that represents a warning message for the pulling MSH to indicate that no AS4 user messages are available to be pulled.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        public static Error CreatePullRequestWarning(string messageId)
        {
            return new Error(
                messageId,
                refToMessageId: null,
                line: new ErrorLine(
                    ErrorCode.Ebms0006,
                    Severity.WARNING,
                    ErrorAlias.EmptyMessagePartitionChannel));
        }

        /// <summary>
        /// Creates a new <see cref="Error"/> model from an <see cref="ErrorResult"/> instance.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to an ebMS message identifier of an <see cref="Core.UserMessage"/>.</param>
        /// <param name="result">The occured error to create an ebMS error message from.</param>
        public static Error FromErrorResult(string messageId, string refToMessageId, ErrorResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            return new Error(messageId, refToMessageId, ErrorLine.FromErrorResult(result));
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Error"/> is a response of a pull request.
        /// </summary>
        [XmlIgnore]
        public bool IsPullRequestWarning
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

    public class ErrorLine : IEquatable<ErrorLine>
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
        internal ErrorLine(
            ErrorCode errorCode,
            Severity severity,
            ErrorAlias shortDescription,
            Maybe<string> origin,
            Maybe<string> category,
            Maybe<string> refToMessageInError,
            Maybe<ErrorDescription> description,
            Maybe<string> detail)
        {
            ErrorCode = errorCode;
            Severity = severity;
            Origin = origin ?? Maybe<string>.Nothing;
            Category = category ?? Maybe<string>.Nothing;
            RefToMessageInError = refToMessageInError ?? Maybe<string>.Nothing;
            ShortDescription = shortDescription;
            Description = description ?? Maybe<ErrorDescription>.Nothing;
            Detail = detail ?? Maybe<string>.Nothing;
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(ErrorLine other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ErrorCode == other.ErrorCode
                   && Severity == other.Severity
                   && Origin.Equals(other.Origin)
                   && Category.Equals(other.Category)
                   && RefToMessageInError.Equals(other.RefToMessageInError)
                   && ShortDescription == other.ShortDescription
                   && Description.Equals(other.Description)
                   && Detail.Equals(other.Detail);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is ErrorLine l && Equals(l);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) ErrorCode;
                hashCode = (hashCode * 397) ^ (int) Severity;
                hashCode = (hashCode * 397) ^ Origin.GetHashCode();
                hashCode = (hashCode * 397) ^ Category.GetHashCode();
                hashCode = (hashCode * 397) ^ RefToMessageInError.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) ShortDescription;
                hashCode = (hashCode * 397) ^ Description.GetHashCode();
                hashCode = (hashCode * 397) ^ Detail.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.ErrorLine" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(ErrorLine left, ErrorLine right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.ErrorLine" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(ErrorLine left, ErrorLine right)
        {
            return !Equals(left, right);
        }
    }

    public class ErrorDescription : IEquatable<ErrorDescription>
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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(ErrorDescription other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return String.Equals(Language, other.Language)
                   && String.Equals(Value, other.Value);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is ErrorDescription d && Equals(d);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Language.GetHashCode() * 397) ^ Value.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.ErrorDescription" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(ErrorDescription left, ErrorDescription right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.ErrorDescription" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(ErrorDescription left, ErrorDescription right)
        {
            return !Equals(left, right);
        }
    }

    public enum Severity
    {
        FAILURE,
        WARNING
    }
}