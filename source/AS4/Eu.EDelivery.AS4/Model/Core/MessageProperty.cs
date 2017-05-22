using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public sealed class MessageProperty : IEquatable<MessageProperty>
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public MessageProperty() : this(null, null)
        {
            // Default ctor is necessary for XML Serialization.   
        }

        public MessageProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public bool Equals(MessageProperty other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name) &&
                   StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MessageProperty;

            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode() ^ 42 +
                   (Value ?? "").GetHashCode() ^ 23;
        }
    }
}