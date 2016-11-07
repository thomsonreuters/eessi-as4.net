namespace Eu.EDelivery.AS4.Model.Core
{
    public class PullRequest : SignalMessage
    {
        public string Mpc { get; set; }

        public PullRequest() {}

        public PullRequest(string mpc)
        {
            this.Mpc = mpc;
        }
    }
}