using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Factories;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class UserMessage : MessageUnit
    {
        private readonly ICollection<PartInfo> _partInfos;
        private readonly ICollection<MessageProperty> _messageProperties;

        public string Mpc { get; set; }

        public Party Sender { get; set; }

        public Party Receiver { get; set; }

        public CollaborationInfo CollaborationInfo { get; set; }

        public IEnumerable<MessageProperty> MessageProperties => _messageProperties.AsEnumerable();

        public IEnumerable<PartInfo> PayloadInfo => _partInfos.AsEnumerable();

        // TODO: this should happen together with the adding of Attachments
        // TODO: only unique PartInfo's are allowed
        public void AddPartInfo(PartInfo p)
        {
            if (p == null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            _partInfos.Add(p);
        }

        public void AddMessageProperty(MessageProperty p)
        {
            if (p == null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            _messageProperties.Add(p);
        }

        public void AddMessageProperties(IEnumerable<MessageProperty> props)
        {
            if (props == null)
            {
                throw new ArgumentNullException(nameof(props));
            }

            foreach (MessageProperty p in props)
            {
                AddMessageProperty(p);
            }
        }

        public void ClearMessageProperties()
        {
            _messageProperties.Clear();
        }

        [XmlIgnore]
        public bool IsDuplicate { get; set; }

        [XmlIgnore]
        public bool IsTest { get; set; }

        public bool ShouldSerializePayloadInfo()
        {
            // When this method returns false, the XmlSerializer will not write an XmlElement for the PayloadInfo attribute.
            return _partInfos.Any();
        }

        public UserMessage() : this(IdentifierFactory.Instance.Create())
        {

        }

        public UserMessage(string messageId) : base(messageId)
        {
            CollaborationInfo = new CollaborationInfo(
                agreement: Maybe<AgreementReference>.Nothing,
                service: Service.TestService,
                action: Constants.Namespaces.TestAction,
                conversationId: CollaborationInfo.DefaultConversationId);
            Sender = new Party(Constants.Namespaces.EbmsDefaultFrom, new PartyId(Constants.Namespaces.EbmsDefaultFrom));
            Receiver = new Party(Constants.Namespaces.EbmsDefaultTo, new PartyId(Constants.Namespaces.EbmsDefaultTo));   

            _partInfos = new Collection<PartInfo>();
            _messageProperties = new Collection<MessageProperty>();
        }

        public UserMessage(
            string messageId, 
            string refToMessageId, 
            CollaborationInfo collaboration, 
            Party sender, 
            Party receiver, 
            IEnumerable<PartInfo> partInfos, 
            IEnumerable<MessageProperty> props) : base(messageId, refToMessageId)
        {
            if (collaboration == null)
            {
                throw new ArgumentNullException(nameof(collaboration));
            }

            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (receiver == null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (partInfos == null || partInfos.Any(p => p is null))
            {
                throw new ArgumentNullException(nameof(partInfos));
            }

            if (props == null || props.Any(p => p is null))
            {
                throw new ArgumentNullException(nameof(props));
            }

            CollaborationInfo = collaboration;
            Sender = sender;
            Receiver = receiver;

            _partInfos = partInfos.ToList();
            _messageProperties = props.ToList();
        }


        public override string ToString()
        {
            return $"UserMessage [${this.MessageId}]";
        }
    }
}