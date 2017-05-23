using System;

namespace Eu.EDelivery.AS4.Model.Common
{
    public class MessageProperty : IEquatable<MessageProperty>
    {
        public string Name { get; set; }        
        public string Value { get; set; }

        public MessageProperty() : this(string.Empty, string.Empty)
        {
            // Default constructor is necessary for serialization.
        }

        public MessageProperty(string name, string value)
        {
            Name = name;            
            Value = value;
        }

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
                string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&                
                string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
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
                int hashCode = Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Name) : 0;
                
                hashCode = (hashCode * 397) ^
                           (Value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Value) : 0);
                return hashCode;
            }
        }
    }
}