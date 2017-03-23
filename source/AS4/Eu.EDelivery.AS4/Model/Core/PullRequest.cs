namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// AS4 Pull Request Signal Message.
    /// </summary>
    public class PullRequest : SignalMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequest" /> class.
        /// Used to serialize the <see cref="PullRequest"/>.
        /// </summary>
        public PullRequest() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequest"/> class.
        /// Used to programmatically initiate a <see cref="PullRequest"/> class.
        /// </summary>
        /// <param name="mpc"></param>
        public PullRequest(string mpc)
        {
            Mpc = mpc;
        }

        public string Mpc { get; set; }
    }
}