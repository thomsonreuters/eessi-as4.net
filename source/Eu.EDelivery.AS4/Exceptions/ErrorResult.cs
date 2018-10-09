using System;

namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// Error model to indicate the occurence of an AS4 Error during the execution of steps.
    /// </summary>
    public class ErrorResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResult" /> class.
        /// </summary>
        /// <param name="description">The description to give the AS4 Error.</param>
        /// <param name="alias">The short description or alias for the AS4 Error Code.</param>
        public ErrorResult(string description, ErrorAlias alias)
        {
            if (String.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(@"Error description should not be blank", nameof(description));
            }

            Description = description;
            Code = ErrorCodeUtils.GetErrorCode(alias);
            Alias = alias;
        }

        /// <summary>
        /// Gets the description for this error result.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the Ebms Error Code for this error result.
        /// </summary>
        public ErrorCode Code { get; }

        /// <summary>
        /// Gets the short description or alias for this error result.
        /// </summary>
        public ErrorAlias Alias { get; }
    }
}
