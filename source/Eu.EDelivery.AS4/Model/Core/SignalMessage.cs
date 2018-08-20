using System;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Model.Core
{
    public abstract class SignalMessage : MessageUnit
    {
        public const string RoutingInputKey = "RoutingInput";

        /// <summary>
        /// Gets or sets whether or not this <see cref="SignalMessage"/> is a duplicated one.
        /// Meaning that the MSH has already processed this message.
        /// </summary>
        [XmlIgnore] public bool IsDuplicate { get; set; }

        /// <summary>
        /// Gets the multihop action value.
        /// </summary>
        public virtual string MultihopAction { get; } = String.Empty;

        /// <summary>
        /// Gets the multihop routing usermessage.
        /// </summary>
        public Maybe<RoutingInputUserMessage> MultiHopRouting { get; } = Maybe<RoutingInputUserMessage>.Nothing;

        /// <summary>
        /// Gets the value indicating whether or not this <see cref="SignalMessage"/> is a multihop message.
        /// </summary>
        public bool IsMultihopSignal => MultiHopRouting != Maybe<RoutingInputUserMessage>.Nothing;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessage"/> class.
        /// </summary>
        /// <remarks>Empty constructor is needed for the <see cref="PullRequest"/> model.</remarks>
        protected SignalMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        protected SignalMessage(string messageId) : base(messageId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        protected SignalMessage(string messageId, string refToMessageId) : base(messageId, refToMessageId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        protected SignalMessage(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp)
            : base(messageId, refToMessageId, timestamp) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="timestamp"></param>
        /// <param name="routing"></param>
        protected SignalMessage(
            string messageId,
            string refToMessageId,
            DateTimeOffset timestamp,
            RoutingInputUserMessage routing)
            : base(messageId, refToMessageId, timestamp)
        {
            MultiHopRouting = Maybe.Just(routing);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalMessage"/> class.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="refToMessageId"></param>
        /// <param name="routedUserMessage"></param>
        protected SignalMessage(
            string messageId, 
            string refToMessageId, 
            RoutingInputUserMessage routedUserMessage)
            : base(messageId, refToMessageId)
        {
            MultiHopRouting = Maybe.Just(routedUserMessage);
        } 
    }
}