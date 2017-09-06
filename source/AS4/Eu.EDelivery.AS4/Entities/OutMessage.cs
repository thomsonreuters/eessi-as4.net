namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Outgoing Message Data Entity Schema
    /// </summary>
    public class OutMessage : MessageEntity
    {
        // ReSharper disable once UnusedMember.Local : Default ctor is required for EF
        private OutMessage()
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessage"/> class.
        /// </summary>
        internal OutMessage(string ebmsMessageId) : base()
        {
            // Internal ctor to prevent that instances are created directly.
            EbmsMessageId = ebmsMessageId;
            SetStatus(default(OutStatus));
        }

        public void SetStatus(OutStatus status)
        {
            Status = status.ToString();
        }

    }
}