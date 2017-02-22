using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public sealed class MessageProperty : IEquatable<MessageProperty>
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public MessageProperty() : this(null, null, null)
        {
            // Default ctor is necessary for XML Serialization.   
        }

        public MessageProperty(string name, string value) : this(name, null, value)
        {
        }

        public MessageProperty(string name, string type, string value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
        }

        public bool Equals(MessageProperty other)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name) &&
                   StringComparer.OrdinalIgnoreCase.Equals(Type, other.Type) &&
                   StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MessageProperty;

            if (other == null)
            {
                return false;
            }

            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode() ^ 42 +
                   (Type ?? "").GetHashCode() ^ 23 +
                   (Value ?? "").GetHashCode() ^ 17;
        }
    }
}