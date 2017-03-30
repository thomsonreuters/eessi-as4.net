using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// <see cref="ReceivedMessage"/> to receive a <see cref="AS4.Entities.MessageEntity"/>
    /// </summary>
    public class ReceivedMessageEntityMessage : ReceivedMessage
    {
        public MessageEntity MessageEntity { get; set; }

        public ReceivedMessageEntityMessage(MessageEntity messageEntity)
        {
            this.MessageEntity = messageEntity;
        }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage"/>
        /// </summary>
        /// <param name="message"></param>
        public override void AssignPropertiesTo(AS4Message message)
        {
            base.AssignPropertiesTo(message);

            message.SendingPMode = GetPMode<SendingProcessingMode>();
            message.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
        }

        public T GetPMode<T>() where T : class
        {
            return AS4XmlSerializer.FromString<T>(this.MessageEntity.PMode);
        }
    }
}