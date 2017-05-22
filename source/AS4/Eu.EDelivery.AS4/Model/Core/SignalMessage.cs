using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.Core
{
    public abstract class SignalMessage : MessageUnit
    {
        [XmlIgnore] public bool IsDuplicated { get; set; }

        protected SignalMessage() {}
        protected SignalMessage(string messageId) : base(messageId) {}

        public virtual string GetActionValue()
        {
            return string.Empty;
        }

        /// <summary>
        /// Contains the UserMessage for which this is a signalmessage.
        /// In MultiHop scenario's, we'll need to make use of this.
        /// </summary>
        [XmlIgnore]
        public UserMessage RelatedUserMessageForMultihop { get; set; }
    }
}