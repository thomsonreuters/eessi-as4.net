using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// AS4 Pull Request Signal Message.
    /// </summary>
    public class PullRequest : SignalMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequest"/> class.
        /// Used to programmatically initiate a <see cref="PullRequest"/> class.
        /// </summary>
        /// <param name="mpc"></param>
        public PullRequest(string mpc)
        {
            Mpc = String.IsNullOrWhiteSpace(mpc) ? Constants.Namespaces.EbmsDefaultMpc : mpc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequest"/> class.
        /// </summary>
        /// <param name="mpc">The mpc.</param>
        /// <param name="messageId">The message Id.</param>
        public PullRequest(string mpc, string messageId) : base(messageId)
        {
            Mpc = String.IsNullOrWhiteSpace(mpc) ? Constants.Namespaces.EbmsDefaultMpc : mpc;
        }

        public string Mpc { get; }
    }
}