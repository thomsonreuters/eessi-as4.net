using System;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using PartyInfo = Eu.EDelivery.AS4.Model.Common.PartyInfo;

namespace Eu.EDelivery.AS4.Model.Submit
{
    [XmlRoot(Namespace = "urn:cef:edelivery:eu:as4:messages")]
    public class SubmitMessage : IMessage
    {
        public MessageInfo MessageInfo { get; set; }
        public PartyInfo PartyInfo { get; set; }
        public CollaborationInfo Collaboration { get; set; }
        public MessageProperty[] MessageProperties { get; set; }
        public Payload[] Payloads { get; set; }
        
        [XmlIgnore]
        public SendingProcessingMode PMode { get; set; }

        [XmlIgnore]
        public bool IsEmpty => String.IsNullOrWhiteSpace(Collaboration?.AgreementRef.PModeId);

        public bool HasPayloads => Payloads != null && Payloads?.Length != 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitMessage"/> class. 
        /// Create Submit Message
        /// </summary>
        public SubmitMessage()
        {            
            MessageInfo = new MessageInfo();
            Collaboration = new CollaborationInfo();
            Payloads = new Payload[] { };
            PartyInfo = new PartyInfo();
        }
    }
}