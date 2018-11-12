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

                if (Entity is ExceptionEntity ee)
                {
                    return AS4XmlSerializer.FromString<T>(ee.PMode);
                }

                return null;
            }

            switch (Entity)
            {
                case InMessage im:
                    switch (im.EbmsMessageType)
                    {
                        case MessageType.Receipt:
                        case MessageType.Error:
                            messagingContext.SendingPMode = GetPMode<SendingProcessingMode>();
                            break;
                        case MessageType.UserMessage:
                            messagingContext.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
                            break;
                    }

                    break;
                case OutMessage om:
                    if (om.Intermediary)
                    {
                        messagingContext.SendingPMode = GetPMode<SendingProcessingMode>();
                    }
                    else
                    {
                        switch (om.EbmsMessageType)
                        {
                            case MessageType.Receipt:
                            case MessageType.Error:
                                messagingContext.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
                                break;
                            case MessageType.UserMessage:
                                messagingContext.SendingPMode = GetPMode<SendingProcessingMode>();
                                break;
                        }
                    }

                    break;
                case InException _:
                    messagingContext.ReceivingPMode = GetPMode<ReceivingProcessingMode>();
                    break;
                case OutException _:
                    messagingContext.SendingPMode = GetPMode<SendingProcessingMode>();
                    break;
            }
        }
    }
}