using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Resources;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Model.Internal
{
    /// <summary>
    /// <see cref="ReceivedMessage"/> to receive a <see cref="Entity"/>
    /// </summary>
    public class ReceivedEntityMessage : ReceivedMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// </summary>
        /// <param name="entity"> </param>
        public ReceivedEntityMessage(Entity entity) 
            : this(entity, Stream.Null, string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="contentType"></param>
        public ReceivedEntityMessage(Entity entity, string contentType) 
            : this(entity, Stream.Null, contentType) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedEntityMessage"/> class. 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="underlyingStream"></param>
        /// <param name="contentType"></param>
        public ReceivedEntityMessage(Entity entity, Stream underlyingStream, string contentType) : base(underlyingStream, contentType)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Entity = entity;
        }

        public Entity Entity { get; }

        /// <summary>
        /// Assign custom properties to the <see cref="ReceivedMessage" />
        /// </summary>
        /// <param name="messagingContext"></param>
        public override void AssignPropertiesTo(MessagingContext messagingContext)
        {
            // TODO: can this be moved to somewhere else? Maybe somewhere close to where we explicitly use 'ReceivedEntityMessages'?

            T GetPMode<T>() where T : class
            {
                if (Entity is MessageEntity me)
                {
                    return AS4XmlSerializer.FromString<T>(me.PMode);
                }

                return null;
            }

            if (Entity is InMessage)
            {
                messagingContext.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
                messagingContext.SendingPMode = null;
            }
            else if (Entity is OutMessage om)
            {
                if (om.EbmsMessageType == MessageType.UserMessage)
                {
                    messagingContext.ReceivingPMode = null;
                    messagingContext.SendingPMode = GetPMode<SendingProcessingMode>();
                }
                else if (om.EbmsMessageType == MessageType.Receipt 
                         || om.EbmsMessageType == MessageType.Error)
                {
                    if (om.Intermediary)
                    {
                        // Signal messages that forwarded uses also a sending pmode like user messages to sent the the next MSH.
                        messagingContext.ReceivingPMode = null;
                        messagingContext.SendingPMode = GetPMode<SendingProcessingMode>();
                    }
                    else
                    {
                        // Signal messages that are sent outbound are asynchronous messages that needs the receiving pmode for response information.
                        messagingContext.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
                        messagingContext.SendingPMode = null;
                    }
                }
            }
        }
    }
}