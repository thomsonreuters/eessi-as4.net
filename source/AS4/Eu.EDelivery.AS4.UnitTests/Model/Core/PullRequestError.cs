using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.UnitTests.Model.Core
{
    /// <summary>
    /// <see cref="Error"/> implementation to define the specifics for a error returning from a Pull Request.
    /// </summary>
    public class PullRequestError : Error
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestError"/> class.
        /// </summary>
        public PullRequestError()
        {
            Errors = new List<ErrorDetail>
            {
                new ErrorDetail {Severity = Severity.WARNING, ShortDescription = "EmptyMessagePartitionChannel"}
            };
        }
    }
}