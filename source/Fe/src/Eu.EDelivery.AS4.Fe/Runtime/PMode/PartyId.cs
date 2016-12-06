using System;

namespace Eu.EDelivery.AS4.Model.PMode
{
    public class PartyId : IEquatable<PartyId>
    {
        public string Id { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId"/> class
        /// </summary>
        public PartyId() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId"/> class
        /// with a given <paramref name="id"/>
        /// </summary>
        /// <param name="id"></param>
        public PartyId(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(PartyId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                string.Equals(this.Id, other.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Type, other.Type, StringComparison.OrdinalIgnoreCase);
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

            return obj.GetType() == GetType() && Equals((PartyId)obj);
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
                return
                    ((this.Id != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Id) : 0) * 397) ^
                    (this.Type != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Type) : 0);
            }
        }
    }
}