using System;
using System.ComponentModel.DataAnnotations;
using Eu.EDelivery.AS4.Agents;

namespace Eu.EDelivery.AS4.Entities
{
    public class Journal : IEquatable<Journal>
    {
        public long Id { get; private set; }

        public void InitializeIdFromDatabase(long id)
        {
            Id = id;
        }

        public bool IsTransient => Id == default(long);

        public long? RefToInMessageId { set; get; }

        public long? RefToOutMessageId { get; set; }

        public DateTimeOffset LogDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string AgentType { get; private set; }

        public void SetAgentType(AgentType t)
        {
            AgentType = t.ToString();
        }

        [Required]
        [MaxLength(50)]
        public string AgentName { get; set; }

        [Required]
        [MaxLength(100)]
        public string EbmsMessageId { get; set; }

        [MaxLength(100)]
        public string RefToEbmsMessageId { get; set; }

        [MaxLength(255)]
        public string FromParty { get; set; }

        [MaxLength(255)]
        public string ToParty { get; set; }

        [MaxLength(255)]
        public string Service { get; set; }

        [MaxLength(255)]
        public string Action { get; set; }

        [Required]
        public string LogEntry { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Journal other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
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

            return obj is Journal j && Equals(j);
        }

        private int? _hashCode;

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            if (_hashCode.HasValue)
            {
                return _hashCode.Value;
            }

            _hashCode = IsTransient ? base.GetHashCode() : Id.GetHashCode();

            return _hashCode.Value;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }
}
