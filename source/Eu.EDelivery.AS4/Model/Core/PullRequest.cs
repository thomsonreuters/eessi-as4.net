using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// ebMS signal message unit representing a request for <see cref="UserMessage"/> instances in a pull scenario.
    /// </summary>
    public class PullRequest : SignalMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequest"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier for this message unit.</param>
        /// <param name="mpc">The message partition channel on which the request is pulled.</param>
        public PullRequest(string messageId, string mpc) : base(messageId)
        {
            Mpc = String.IsNullOrWhiteSpace(mpc) ? Constants.Namespaces.EbmsDefaultMpc : mpc;
        }

        /// <summary>
        /// Gets the message partition channel for this message.
        /// </summary>
        public string Mpc { get; }
    }
}