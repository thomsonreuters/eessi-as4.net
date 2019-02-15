using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

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
            : base(ebmsMessageId)
        {
            SetStatus(default(InStatus));
        }

        public void SetStatus(InStatus status)
        {
            Status = status.ToString();
        }

        /// <summary>
        /// Gets the sending processing mode based on a child representation of a message entity.
        /// </summary>
        public override SendingProcessingMode GetSendingPMode()
        {
            if(EbmsMessageType != MessageType.UserMessage && !Intermediary)
            {
                return AS4XmlSerializer.FromString<SendingProcessingMode>(PMode);
            }

            return null;
        }

        /// <summary>
        /// Gets the receiving processing mode based on a child representation of a message entity.
        /// </summary>
        public override ReceivingProcessingMode GetReceivingPMode()
        {
            if (EbmsMessageType == MessageType.UserMessage || Intermediary)
            {
                return AS4XmlSerializer.FromString<ReceivingProcessingMode>(PMode);
            }

            return null;
        }
    }
}