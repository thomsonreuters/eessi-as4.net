using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using NLog;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// AS4 Message Entity
    /// </summary>
    public abstract class MessageEntity : Entity
    {
        public string EbmsMessageId { get; private set; }

        public string EbmsRefToMessageId { get; set; }

        [MaxLength(256)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets a string representation of the PMode that has been used to process this message.
        /// </summary>
        public string PMode { get; private set; }

        /// <summary>
        /// Gets the ID of the PMode that is used to process this message.
        /// </summary>
        public string PModeId { get; private set; }

        /// <summary>
        /// Set the Id & string represenation of the PMode that is used to process the message.
        /// </summary>
        /// <param name="pmodeId"></param>
        /// <param name="pmodeContent"></param>
        public void SetPModeInformation(string pmodeId, string pmodeContent)
        {
            PModeId = pmodeId;
            PMode = pmodeContent;
        }

        /// <summary>
        /// Set the PMode that is used to process the message.
        /// </summary>
        /// <param name="pmode"></param>
        public async Task SetPModeInformationAsync(IPMode pmode)
        {
            if (pmode != null)
            {
                PModeId = pmode.Id;

                // The Xml Serializer is not able to serialize an interface, therefore
                // the argument must first be cast to a correct implementation.

                if (pmode is SendingProcessingMode sp)
                {
                    PMode = await AS4XmlSerializer.ToStringAsync(sp);
                }
                else if (pmode is ReceivingProcessingMode rp)
                {
                    PMode = await AS4XmlSerializer.ToStringAsync(rp);
                }
                else
                {
                    throw new NotImplementedException("Unable to serialize the the specified IPMode");
                }
            }
        }

        [MaxLength(255)]
        public string FromParty { get; set; }

        [MaxLength(255)]
        public string ToParty { get; set; }

        [Column("MPC")]
        [MaxLength(255)]
        public string Mpc { get; set; }

        [MaxLength(50)]
        public string ConversationId { get; set; }

        [MaxLength(255)]
        public string Service { get; set; }

        [MaxLength(255)]
        public string Action { get; set; }

        public bool IsDuplicate { get; set; }

        public bool IsTest { get; set; }

        /// <summary>
        /// Flag that indicates whether or not we have treated this message 
        /// as an Intermediary MSH
        /// </summary>        
        public bool Intermediary { get; set; }

        /// <summary>
        /// Gets to the location where the AS4Message body can be found.
        /// </summary>
        [MaxLength(512)]
        public string MessageLocation { get; set; }

        public void SetOperation(Operation operation)
        {
            Operation = operation.ToString();
        }

        [Column("Operation")]
        [MaxLength(50)]
        public string Operation { get; private set; }


        [Column("MEP")]
        [MaxLength(25)]
        public string MEP { get; private set; }

        public void SetMessageExchangePattern(MessageExchangePattern mep)
        {
            MEP = mep.ToString();
        }

        public string EbmsMessageType { get; private set; }

        public void SetEbmsMessageType(MessageType messageType)
        {
            EbmsMessageType = messageType.ToString();
        }

        [Column("Status")]
        public string Status { get; protected set; }

        public string SoapEnvelope { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEntity"/> class.
        /// </summary>
        protected MessageEntity()
        {
            SetOperation(default(Operation));
            SetEbmsMessageType(default(MessageType));
            SetMessageExchangePattern(default(MessageExchangePattern));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEntity"/> class.
        /// </summary>
        protected MessageEntity(string ebmsMessageId) : this()
        {
            EbmsMessageId = ebmsMessageId;
        }

        /// <summary>
        /// Assigns the parent properties.
        /// </summary>
        /// <param name="messageUnit">The MessageUnit from which the properties must be retrieved..</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void AssignAS4Properties(MessageUnit messageUnit, CancellationToken cancellationToken)
        {
            if (messageUnit is UserMessage userMessage)
            {
                FromParty = userMessage.Sender.PartyIds.First().Id;
                ToParty = userMessage.Receiver.PartyIds.First().Id;
                Action = userMessage.CollaborationInfo.Action;
                Service = userMessage.CollaborationInfo.Service.Value;
                ConversationId = userMessage.CollaborationInfo.ConversationId;
                Mpc = userMessage.Mpc;
                IsTest = userMessage.IsTest;
                IsDuplicate = userMessage.IsDuplicate;
                SoapEnvelope = AS4XmlSerializer.ToString(AS4Mapper.Map<Xml.UserMessage>(userMessage));
            }
            else
            {
                if (messageUnit is SignalMessage signalMessage)
                {
                    IsDuplicate = signalMessage.IsDuplicate;
                    Mpc = signalMessage.MultiHopRouting?.mpc ?? Constants.Namespaces.EbmsDefaultMpc;
                }
            }
        }

        /// <summary>
        /// Update the <see cref="Entity" /> to lock it with a given <paramref name="value" />.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity" /> is locked.</param>
        public override void Lock(string value)
        {
            var updatedOperation = OperationUtils.Parse(value);

            if (updatedOperation != AS4.Entities.Operation.NotApplicable)
            {
                SetOperation(updatedOperation);
            }
        }

        /// <summary>
        /// Retrieves the Message body as a stream.
        /// </summary>
        /// <param name="store">
        /// The <see cref="MessageBodyStore" /> which is responsible for providing the correct
        /// <see cref="IAS4MessageBodyStore" /> that loads the <see cref="AS4Message" /> body.
        /// </param>
        /// <returns>A Stream which contains the MessageBody</returns>
        public async Task<Stream> RetrieveMessageBody(IAS4MessageBodyStore store)
        {
            if (string.IsNullOrWhiteSpace(MessageLocation))
            {
                LogManager.GetCurrentClassLogger().Warn("Unable to retrieve the AS4 Message Body: MessageLocation is not set.");
                return null;
            }

            return await TryLoadMessageBody(store);
        }

        private async Task<Stream> TryLoadMessageBody(IAS4MessageBodyStore store)
        {
            try
            {
                return await store.LoadMessageBodyAsync(MessageLocation);
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Error(exception.Message);

                return null;
            }
        }
    }
}