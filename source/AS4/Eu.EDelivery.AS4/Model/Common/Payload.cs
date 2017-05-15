using System;

namespace Eu.EDelivery.AS4.Model.Common
{
    public class Payload : IEquatable<Payload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payload" /> class.
        /// Xml Serializer needs a parameterless constructor
        /// </summary>
        public Payload() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Payload" /> class.
        /// Create a <see cref="Payload" /> Model
        /// to a given <paramref name="location" />
        /// </summary>
        /// <param name="location">
        /// </param>
        public Payload(string location)
        {
            Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payload" /> class.
        /// Create a <see cref="Payload" /> Model
        /// to a given <paramref name="location" />
        /// for a given <paramref name="mimeType" />
        /// </summary>
        /// <param name="id">
        /// </param>
        /// <param name="location">
        /// </param>
        /// <param name="mimeType">
        /// </param>
        public Payload(string id, string location, string mimeType)
        {
            Id = id;
            Location = location;
            MimeType = mimeType;
        }

        public string Id { get; set; }

        public string MimeType { get; set; }

        public string CharacterSet { get; set; }

        public string Location { get; set; }

        public Schema[] Schemas { get; set; }

        public PayloadProperty[] PayloadProperties { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Payload other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(MimeType, other.MimeType, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(CharacterSet, other.CharacterSet, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(Location, other.Location, StringComparison.OrdinalIgnoreCase);
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
            return Equals(obj as Payload);
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
                int hashCode = Id != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Id) : 0;

                hashCode = (hashCode * 397)
                           ^ (MimeType != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(MimeType) : 0);
                hashCode = (hashCode * 397)
                           ^ (CharacterSet != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(CharacterSet) : 0);
                hashCode = (hashCode * 397)
                           ^ (Location != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Location) : 0);

                return hashCode;
            }
        }
    }
}