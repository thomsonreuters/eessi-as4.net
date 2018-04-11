using System;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Model.Common
{
    public class Schema : IEquatable<Schema>
    {
        [XmlElement(IsNullable = true)]
        public string Location { get; set; }

        public string Version { get; set; }

        public string Namespace { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class. 
        /// Create a basic <see cref="Schema"/> Model
        /// </summary>
        public Schema() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class. 
        /// Create a <see cref="Schema"/> Model
        /// to a given <paramref name="location"/>
        /// </summary>
        /// <param name="location">
        /// </param>
        public Schema(string location)
        {
            this.Location = location;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Schema other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(this.Location, other.Location, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Version, other.Version, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Namespace, other.Namespace, StringComparison.OrdinalIgnoreCase);
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

            return obj.GetType() == GetType() && Equals((Schema) obj);
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
                int hashCode = this.Location != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Location) : 0;
                hashCode = (hashCode * 397) ^ (this.Version != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Version) : 0);
                hashCode = (hashCode * 397) ^ (this.Namespace != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Namespace) : 0);

                return hashCode;
            }
        }
    }
}