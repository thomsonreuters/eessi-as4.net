namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    ///     Incoming Message Data Entity Schema
    /// </summary>
    public class InMessage : MessageEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMessage"/> class.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - Default ctor is required for Entity Framework.
        private InMessage()
        {
        }

        public InMessage(string ebmsMessageId)
        {
            EbmsMessageId = ebmsMessageId;
            SetStatus(default(InStatus));
        }

        public void SetStatus(InStatus status)
        {
            Status = status.ToString();
        }
    }
}