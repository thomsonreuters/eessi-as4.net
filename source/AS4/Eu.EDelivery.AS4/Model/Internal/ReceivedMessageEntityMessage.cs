using System.IO;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// <see cref="ReceivedMessage"/> to receive a <see cref="AS4.Entities.MessageEntity"/>
    /// </summary>
    public class ReceivedMessageEntityMessage : ReceivedEntityMessage 
    {
        public MessageEntity MessageEntity { get; }

        public ReceivedMessageEntityMessage(MessageEntity messageEntity, Stream underlyingStream, string contentType) 
            : base(messageEntity, underlyingStream, contentType)
        {
            MessageEntity = messageEntity;
        }

        public override void AssignPropertiesTo(MessagingContext messagingContext)
        {
            base.AssignPropertiesTo(messagingContext);

            if (MessageEntity is InMessage)
            {
                messagingContext.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
                messagingContext.SendingPMode = null;
            }
            else if (MessageEntity is OutMessage)
            {
                messagingContext.ReceivingPMode = null;
                messagingContext.SendingPMode = GetPMode<SendingProcessingMode>();
            }
        }

        public T GetPMode<T>() where T : class
        {
            return AS4XmlSerializer.FromString<T>(this.MessageEntity.PMode);
        }
    }
}