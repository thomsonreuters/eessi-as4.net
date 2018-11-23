using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// ebMS collaboration information of an <see cref="UserMessage"/> to identify the agreed agreement between two parties.
    /// </summary>
    public class CollaborationInfo : IEquatable<CollaborationInfo>
    {
        /// <summary>
        /// Gets the reference of an agreement between two parties.
        /// </summary>
        public Maybe<AgreementReference> AgreementReference { get; }

        /// <summary>
        /// Gets the service which acts on the message.
        /// </summary>
        public Service Service { get; }

        /// <summary>
        /// Gets the action within a service that acts on the message.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Gets the conversation identifier of this message.
        /// </summary>
        public string ConversationId { get; }

        /// <summary>
        /// Gets the default conversation identifier for messages.
        /// </summary>
        public static readonly string DefaultConversationId = "1";

        /// <summary>
        /// Gets the default collaboration information for test messages.
        /// </summary>
        public static readonly CollaborationInfo DefaultTest = 
            new CollaborationInfo(
                Maybe<AgreementReference>.Nothing, 
                Service.TestService, 
                Constants.Namespaces.TestAction,
                DefaultConversationId);

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="agreement">The reference of an agreement between two parties.</param>
        public CollaborationInfo(AgreementReference agreement)
            : this(
                agreement: agreement,
                service: Service.TestService,
                action: Constants.Namespaces.TestAction,
                conversationId: DefaultConversationId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="service">The service which acts on the message.</param>
        internal CollaborationInfo(Service service) 
            : this(
                Maybe<AgreementReference>.Nothing, 
                service, 
                Constants.Namespaces.TestAction, 
                DefaultConversationId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="service">The service which acts on the message.</param>
        /// <param name="action">The action within a service that acts on the message.</param>
        internal CollaborationInfo(Service service, string action)
            : this(
                Maybe<AgreementReference>.Nothing,
                service,
                action,
                DefaultConversationId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="agreement">The reference of an agreement between two parties.</param>
        /// <param name="service">The service which acts on the message.</param>
        /// <param name="action">The action within a service that acts on the message.</param>
        /// <param name="conversationId">The conversation identifier of the message.</param>
        public CollaborationInfo(
            AgreementReference agreement,
            Service service,
            string action,
            string conversationId)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            AgreementReference = (agreement != null).ThenMaybe(agreement);
            Service = service;
            Action = action;
            ConversationId = conversationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class.
        /// </summary>
        /// <param name="agreement">The reference of an agreement between two parties.</param>
        /// <param name="service">The service which acts on the message.</param>
        /// <param name="action">The action within a service that acts on the message.</param>
        /// <param name="conversationId">The conversation identifier of the message.</param>
        internal CollaborationInfo(
            Maybe<AgreementReference> agreement, 
            Service service, 
            string action, 
            string conversationId)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (conversationId == null)
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            AgreementReference = agreement ?? Maybe<AgreementReference>.Nothing;
            Service = service;
            Action = action;
            ConversationId = conversationId;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(CollaborationInfo other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return AgreementReference.Equals(other.AgreementReference)
                   && Service.Equals(other.Service)
                   && String.Equals(Action, other.Action)
                   && String.Equals(ConversationId, other.ConversationId);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is CollaborationInfo c && Equals(c);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = AgreementReference.GetHashCode();
                hashCode = (hashCode * 397) ^ Service.GetHashCode();
                hashCode = (hashCode * 397) ^ Action.GetHashCode();
                hashCode = (hashCode * 397) ^ ConversationId.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.CollaborationInfo" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(CollaborationInfo left, CollaborationInfo right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.CollaborationInfo" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(CollaborationInfo left, CollaborationInfo right)
        {
            return !Equals(left, right);
        }
    }
}