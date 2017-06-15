using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;

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
        {
            Errors = new List<ErrorDetail>
            {
                new ErrorDetail
                {
                    ErrorCode = $"EBMS:{(int) ErrorCode.Ebms0006:0000}",
                    Severity = Severity.WARNING,
                    ShortDescription = "EmptyMessagePartitionChannel"
                }
            };
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(PullRequestError other)
        {
            ErrorDetail thisDetail = Errors.First();
            ErrorDetail otherDetail = other.Errors.First();

            return thisDetail.ErrorCode == otherDetail.ErrorCode 
                   && thisDetail.Severity == otherDetail.Severity
                   && thisDetail.ShortDescription == otherDetail.ShortDescription;
        }
    }
}