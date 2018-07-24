using System;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// <see cref="Error"/> implementation to define the specifics for a error returning from a Pull Request.
    /// </summary>
    public class PullRequestError : Error, IEquatable<PullRequestError>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestError"/> class.
        /// </summary>
        public PullRequestError() 
            : base(
                messageId: null, 
                refToMessageId: null, 
                line: new ErrorLine(
                    ErrorCode.Ebms0006,
                    Severity.WARNING,
                    ErrorAlias.EmptyMessagePartitionChannel)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestError"/> class.
        /// </summary>
        /// <param name="refToMessageId"></param>
        public PullRequestError(string refToMessageId)
            : base(
                IdentifierFactory.Instance.Create(),
                refToMessageId,
                new ErrorLine(
                    ErrorCode.Ebms0006,
                    Severity.WARNING,
                    ErrorAlias.EmptyMessagePartitionChannel)) { }

        /// <summary>I
        /// ndicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(PullRequestError other)
        {
            ErrorLine thisDetail = ErrorLines.First();
            ErrorLine otherDetail = other.ErrorLines.First();

            return thisDetail.ErrorCode == otherDetail.ErrorCode 
                   && thisDetail.Severity == otherDetail.Severity
                   && thisDetail.ShortDescription == otherDetail.ShortDescription;
        }
    }
}