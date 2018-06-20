using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class PartyId : IEquatable<PartyId>
    {
        public string Id { get; }

        public string Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        public PartyId() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id"></param>
        public PartyId(string id) : this(id, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        public PartyId(string id, string type)
        {
            Id = id;
            Type = type;
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
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Type ?? string.Empty, other.Type ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is PartyId other && Equals(other);
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
                    ((Id != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Id) : 0) * 397)
                    ^ (Type != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Type) : 0);
            }
        }
    }
}