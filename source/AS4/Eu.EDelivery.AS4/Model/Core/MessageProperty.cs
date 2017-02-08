namespace Eu.EDelivery.AS4.Model.Core
{
    public sealed class MessageProperty
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
    }
}