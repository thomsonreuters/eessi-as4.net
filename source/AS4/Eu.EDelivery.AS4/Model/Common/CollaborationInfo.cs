using System;

namespace Eu.EDelivery.AS4.Model.Common
{
    public class CollaborationInfo : IEquatable<CollaborationInfo>
    {
        public string Action { get; set; }
        public string ConversationId { get; set; }
        public Agreement AgreementRef { get; set; }
        public Service Service { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class. 
        /// Create a basic <see cref="CollaborationInfo"/> Model
        /// </summary>
        public CollaborationInfo()
        {
            this.AgreementRef = new Agreement();
            this.Service = new Service();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class. 
        /// Create a <see cref="CollaborationInfo"/> Model
        /// with a given <paramref name="conversationId"/> 
        /// and <paramref name="agreement"/>
        /// </summary>
        /// <param name="conversationId">
        /// </param>
        /// <param name="agreement">
        /// </param>
        public CollaborationInfo(string conversationId, Agreement agreement)
        {
            this.ConversationId = conversationId;
            this.AgreementRef = agreement;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class. 
        /// Create a <see cref="CollaborationInfo"/> Model
        /// with a given <paramref name="conversationId"/>, <paramref name="agreement"/> and <paramref name="service"/>
        /// </summary>
        /// <param name="conversationId">
        /// </param>
        /// <param name="agreement">
        /// </param>
        /// <param name="service">
        /// </param>
        public CollaborationInfo(string conversationId, Agreement agreement, Service service)
        {
            this.ConversationId = conversationId;
            this.AgreementRef = agreement;
            this.Service = service;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(CollaborationInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                string.Equals(this.Action, other.Action, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.ConversationId, other.ConversationId, StringComparison.OrdinalIgnoreCase) &&
                Equals(this.AgreementRef, other.AgreementRef) && Equals(this.Service, other.Service);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((CollaborationInfo) obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Action != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Action) : 0;
                hashCode = (hashCode * 397)
                           ^ (this.ConversationId != null
                                  ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.ConversationId)
                               : 0);
                hashCode = (hashCode * 397) ^ (this.AgreementRef?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Service?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}