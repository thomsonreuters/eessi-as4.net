using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Message which contains the actual business payload that is exchanged amongst the business applications of two parties.
    /// </summary>
    [DebuggerDisplay("UserMessage " + nameof(MessageId))]
    public class UserMessage : MessageUnit
    {
        /// <summary>
        /// Gets the message partition channel for this message.
        /// </summary>
        public string Mpc { get; }

        /// <summary>
        /// Gets the sender of the message.
        /// </summary>
        public Party Sender { get; }

        /// <summary>
        /// Gets the receiver of the message.
        /// </summary>
        public Party Receiver { get; }

        /// <summary>
        /// Gets the information which describes the business context through a service and action parameter.
        /// </summary>
        public CollaborationInfo CollaborationInfo { get; }

        /// <summary>
        /// Gets the properties which offer an extension point to add additional business information.
        /// </summary>
        public IEnumerable<MessageProperty> MessageProperties { get; }

        /// <summary>
        /// Gets the information which describes the reference to the payloads in the SOAP Body or Attachments.
        /// </summary>
        public IEnumerable<PartInfo> PayloadInfo { get; }

        /// <summary>
        /// Gets or sets the value indicating whether or not this message is a duplicate one.
        /// </summary>
        /// <remarks>This property remains to have a public setter because other possible stateless systems needs to have a way to flag this also.</remarks>
        [XmlIgnore]
        internal bool IsDuplicate { get; set; }

        /// <summary>
        /// Gets the value indicating if this message is a test message.
        /// </summary>
        [XmlIgnore]
        public bool IsTest { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        public UserMessage(string messageId) 
            : this(
                messageId, 
                Constants.Namespaces.EbmsDefaultMpc, 
                CollaborationInfo.DefaultTest, 
                Party.DefaultFrom, 
                Party.DefaultTo, 
                new PartInfo[0], 
                new MessageProperty[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="mpc">The message partition channel of this message unit.</param>
        public UserMessage(string messageId, string mpc) : this(messageId)
        {
            Mpc = mpc ?? Constants.Namespaces.EbmsDefaultMpc;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="collaboration">The information which describes the business context through a service and action parameter.</param>
        internal UserMessage(string messageId, CollaborationInfo collaboration)
            : this(messageId, collaboration, Party.DefaultFrom, Party.DefaultTo, new PartInfo[0], new MessageProperty[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="sender">The agreed sender of this user message.</param>
        /// <param name="receiver">The agreed receiver of this user message.</param>
        internal UserMessage(string messageId, Party sender, Party receiver)
            : this(
                messageId, 
                CollaborationInfo.DefaultTest, 
                sender, 
                receiver,
                new PartInfo[0],
                new MessageProperty[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        internal UserMessage(string messageId, PartInfo part) 
            : this(messageId, CollaborationInfo.DefaultTest, Party.DefaultFrom, Party.DefaultTo, new [] { part }, new MessageProperty[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="collaboration">The information which describes the business context through a service and action parameter.</param>
        /// <param name="sender">The agreed sender of this user message.</param>
        /// <param name="receiver">The agreed receiver of this user message.</param>
        /// <param name="partInfos">The information which describes the reference to the payloads in the SOAP Body or attachments.</param>
        /// <param name="messageProperties">The properties which offer an extension point to add additional business information.</param>
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
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="mpc">The message partition channel of this message unit.</param>
        /// <param name="collaboration">The information which describes the business context through a service and action parameter.</param>
        /// <param name="sender">The agreed sender of this user message.</param>
        /// <param name="receiver">The agreed receiver of this user message.</param>
        /// <param name="partInfos">The information which describes the reference to the payloads in the SOAP Body or attachments.</param>
        /// <param name="messageProperties">The properties which offer an extension point to add additional business information.</param>
        public UserMessage(
            string messageId,
            string mpc,
            CollaborationInfo collaboration,
            Party sender,
            Party receiver,
            IEnumerable<PartInfo> partInfos,
            IEnumerable<MessageProperty> messageProperties) 
            : this(messageId, null, DateTimeOffset.Now, mpc, collaboration, sender, receiver, partInfos, messageProperties) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMessage"/> class.
        /// </summary>
        /// <param name="messageId">The ebMS message identifier of this message unit.</param>
        /// <param name="refToMessageId">The reference to another ebMS message unit.</param>
        /// <param name="timestamp">The timestamp when this message is created.</param>
        /// <param name="mpc">The message partition channel of this message unit.</param>
        /// <param name="collaboration">The information which describes the business context through a service and action parameter.</param>
        /// <param name="sender">The agreed sender of this user message.</param>
        /// <param name="receiver">The agreed receiver of this user message.</param>
        /// <param name="partInfos">The information which describes the reference to the payloads in the SOAP Body or attachments.</param>
        /// <param name="messageProperties">The properties which offer an extension point to add additional business information.</param>
        internal UserMessage(
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

            if (partInfos == null)
            {
                throw new ArgumentNullException(nameof(partInfos));
            }

            if (partInfos.Any(p => p is null))
            {
                throw new ArgumentException(@"One or more PartInfo elements is a 'null' reference", nameof(partInfos));
            }

            if (messageProperties == null)
            {
                throw new ArgumentNullException(nameof(messageProperties));
            }

            if (messageProperties.Any(p => p is null))
            {
                throw new ArgumentException(@"One ore more MessageProperty elements is a 'null' reference", nameof(messageProperties));
            }

            Mpc = mpc ?? Constants.Namespaces.EbmsDefaultMpc;
            CollaborationInfo = collaboration;
            Sender = sender;
            Receiver = receiver;

            IsTest = collaboration.Service.Value.Equals(Constants.Namespaces.TestService)
                     && collaboration.Action.Equals(Constants.Namespaces.TestAction);

            PayloadInfo = partInfos.AsEnumerable();
            MessageProperties = messageProperties.AsEnumerable();
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