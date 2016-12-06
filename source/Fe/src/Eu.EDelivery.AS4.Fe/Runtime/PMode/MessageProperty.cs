using System;

namespace Eu.EDelivery.AS4.Model.PMode
{
    public class MessageProperty : IEquatable<MessageProperty>
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(MessageProperty other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                string.Equals(this.Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Type, other.Type, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);
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

            return obj.GetType() == GetType() && Equals((MessageProperty)obj);
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
                int hashCode = this.Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name) : 0;
                hashCode = (hashCode * 397) ^
                           (this.Type != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Type) : 0);
                hashCode = (hashCode * 397) ^
                           (this.Value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value) : 0);
                return hashCode;
            }
        }
    }
}