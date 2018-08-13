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

        public string Mpc { get; }

        public Party Sender { get; }

        public Party Receiver { get; }

        public CollaborationInfo CollaborationInfo { get; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        public UserMessage() : this(IdentifierFactory.Instance.Create()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">Ebms Message Identifier</param>
        public UserMessage(string messageId) : base(messageId)
        {
            Mpc = Constants.Namespaces.EbmsDefaultMpc;

            CollaborationInfo = new CollaborationInfo(
                agreement: Maybe<AgreementReference>.Nothing,
                service: Service.TestService,
                action: Constants.Namespaces.TestAction,
                conversationId: CollaborationInfo.DefaultConversationId);

            Sender = new Party(Constants.Namespaces.EbmsDefaultFrom, new PartyId(Constants.Namespaces.EbmsDefaultFrom));
            Receiver = new Party(Constants.Namespaces.EbmsDefaultTo, new PartyId(Constants.Namespaces.EbmsDefaultTo));

            IsTest = DetermineIfTestMessage(CollaborationInfo.Service, CollaborationInfo.Action);

            _partInfos = new Collection<PartInfo>();
            _messageProperties = new Collection<MessageProperty>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="mpc"></param>
        public UserMessage(string messageId, string mpc) : this(messageId)
        {
            Mpc = mpc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="mpc"></param>
        public UserMessage(string messageId, string refToMessageId, string mpc) : this(messageId, refToMessageId)
        {
            Mpc = mpc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">Ebms Message Identifier</param>
        /// <param name="collaboration">Collaboration information</param>
        public UserMessage(string messageId, CollaborationInfo collaboration)
            : this(messageId, collaboration, Party.DefaultFrom, Party.DefaultTo, new PartInfo[0], new MessageProperty[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        public UserMessage(string messageId, Party sender, Party receiver)
            : this(
                messageId, 
                new CollaborationInfo(
                    Maybe<AgreementReference>.Nothing, 
                    Service.TestService, 
                    Constants.Namespaces.TestAction, 
                    CollaborationInfo.DefaultConversationId), 
                sender, 
                receiver) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="collaboration">Collaboration information</param>
        /// <param name="sender">The sender party</param>
        /// <param name="receiver">The receiver party</param>
        /// <param name="partInfos">The partinfos for the included attachments</param>
        /// <param name="messageProperties">The metadata properties for this message</param>
        public UserMessage(
            CollaborationInfo collaboration,
            Party sender,
            Party receiver,
            IEnumerable<PartInfo> partInfos,
            IEnumerable<MessageProperty> messageProperties)
            : this(IdentifierFactory.Instance.Create(), collaboration, sender, receiver, partInfos, messageProperties) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">Ebms Message Identifier</param>
        /// <param name="collaboration">Collaboration information</param>
        /// <param name="sender">The sender party</param>
        /// <param name="receiver">The receiver party</param>
        public UserMessage(
            string messageId,
            CollaborationInfo collaboration,
            Party sender,
            Party receiver) : this(messageId, collaboration, sender, receiver, new PartInfo[0], new MessageProperty[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">Ebms Message Identifier</param>
        /// <param name="collaboration">Collaboration information</param>
        /// <param name="sender">The sender party</param>
        /// <param name="receiver">The receiver party</param>
        /// <param name="partInfos">The partinfos for the included attachments</param>
        /// <param name="messageProperties">The metadata properties for this message</param>
        public UserMessage(
            string messageId,
            CollaborationInfo collaboration,
            Party sender,
            Party receiver,
            IEnumerable<PartInfo> partInfos,
            IEnumerable<MessageProperty> messageProperties) 
            : this(messageId, Constants.Namespaces.EbmsDefaultMpc, collaboration, sender, receiver, partInfos, messageProperties) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">Ebms Message Identifier</param>
        /// <param name="mpc"></param>
        /// <param name="collaboration">Collaboration information</param>
        /// <param name="sender">The sender party</param>
        /// <param name="receiver">The receiver party</param>
        /// <param name="partInfos">The partinfos for the included attachments</param>
        /// <param name="messageProperties">The metadata properties for this message</param>
        public UserMessage(
            string messageId,
            string mpc,
            CollaborationInfo collaboration,
            Party sender,
            Party receiver,
            IEnumerable<PartInfo> partInfos,
            IEnumerable<MessageProperty> messageProperties) 
            : this(messageId, null, mpc, collaboration, sender, receiver, partInfos, messageProperties) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">Ebms Message Identifier</param>
        /// <param name="refToMessageId"></param>
        /// <param name="mpc"></param>
        /// <param name="collaboration">Collaboration information</param>
        /// <param name="sender">The sender party</param>
        /// <param name="receiver">The receiver party</param>
        /// <param name="partInfos">The partinfos for the included attachments</param>
        /// <param name="messageProperties">The metadata properties for this message</param>
        public UserMessage(
            string messageId,
            string refToMessageId,
            string mpc,
            CollaborationInfo collaboration,
            Party sender,
            Party receiver,
            IEnumerable<PartInfo> partInfos,
            IEnumerable<MessageProperty> messageProperties) 
            : this(messageId, refToMessageId, DateTimeOffset.Now, mpc, collaboration, sender, receiver, partInfos, messageProperties) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">Ebms Message Identifier</param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="mpc"></param>
        /// <param name="collaboration">Collaboration information</param>
        /// <param name="sender">The sender party</param>
        /// <param name="receiver">The receiver party</param>
        /// <param name="partInfos">The partinfos for the included attachments</param>
        /// <param name="messageProperties">The metadata properties for this message</param>
        public UserMessage(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            string mpc,
            CollaborationInfo collaboration,
            Party sender,
            Party receiver,
            IEnumerable<PartInfo> partInfos,
            IEnumerable<MessageProperty> messageProperties) : base(messageId, refToMessageId, timestamp)
        {
            if (mpc == null)
            {
                throw new ArgumentNullException(nameof(mpc));
            }

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

            if (messageProperties == null || messageProperties.Any(p => p is null))
            {
                throw new ArgumentNullException(nameof(messageProperties));
            }

            Mpc = mpc;
            CollaborationInfo = collaboration;
            Sender = sender;
            Receiver = receiver;

            IsTest = DetermineIfTestMessage(collaboration.Service, collaboration.Action);

            _partInfos = partInfos.ToList();
            _messageProperties = messageProperties.ToList();
        }

        private static bool DetermineIfTestMessage(Service service, string action)
        {
            return
                (service.Value?.Equals(Constants.Namespaces.TestService) ?? false) &&
                (action?.Equals(Constants.Namespaces.TestAction) ?? false);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"UserMessage [${MessageId}]";
        }
    }
}