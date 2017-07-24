using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace Eu.EDelivery.AS4.Fe.Pmodes
{
    /// <summary>
    ///     Exception thrown when a PMode is invalid
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidPmodeException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidPmodeException" /> class.
        /// </summary>
        /// <param name="failures">The failures.</param>
        public InvalidPmodeException(IList<ValidationFailure> failures) : base(failures.ToStringList())
        {
        }
    }
}