using System;

namespace Eu.EDelivery.AS4.Model.Common
{
    public class Agreement : IEquatable<Agreement>
    {
        public string Value { get; set; }

        public string RefType { get; set; }

        public string PModeId { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Agreement other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(RefType, other.RefType, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(PModeId, other.PModeId, StringComparison.OrdinalIgnoreCase);
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
            return Equals(obj as Agreement);
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
                int hashCode = Value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Value) : 0;

                hashCode = (hashCode * 397)
                           ^ (RefType != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(RefType) : 0);
                hashCode = (hashCode * 397)
                           ^ (PModeId != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(PModeId) : 0);

                return hashCode;
            }
        }
    }
}