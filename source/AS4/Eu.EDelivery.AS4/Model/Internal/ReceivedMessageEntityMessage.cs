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
        public MessageEntity MessageEntity { get; }

        public ReceivedMessageEntityMessage(MessageEntity messageEntity)
        {
            MessageEntity = messageEntity;
        }

        public override void AssignPropertiesTo(InternalMessage message)
        {
            base.AssignPropertiesTo(message);

            if (MessageEntity is InMessage)
            {
                message.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
                message.SendingPMode = null;
            }
            else if (MessageEntity is OutMessage)
            {
                message.ReceivingPMode = null;
                message.SendingPMode = GetPMode<SendingProcessingMode>();
            }
        }

        public T GetPMode<T>() where T : class
        {
            return AS4XmlSerializer.FromString<T>(this.MessageEntity.PMode);
        }
    }
}