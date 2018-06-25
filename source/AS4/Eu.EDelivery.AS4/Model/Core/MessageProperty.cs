using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Key-Value pair property with a type that's included as a series in the <see cref="AS4Message"/> to define metadata information about the message.
    /// </summary>
    public sealed class MessageProperty : IEquatable<MessageProperty>
    {
        public string Name { get; }

        public string Value { get; }

        public string Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProperty"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public MessageProperty(string name, string value) : this(name, value, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProperty"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public MessageProperty(string name, string value, string type)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));
            }

            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(@"Value cannot be null or empty.", nameof(value));
            }

            Name = name;
            Value = value;
            Type = type;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(MessageProperty other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name)
                   && StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value)
                   && StringComparer.OrdinalIgnoreCase.Equals(Type, other.Type);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is MessageProperty other && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode() ^ 42 +
                   (Type ?? "").GetHashCode() ^ 23 +
                   (Value ?? "").GetHashCode() ^ 17;
        }
    }
}