namespace Eu.EDelivery.AS4.Model.Core
{
    public sealed class MessageProperty
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Value { get; private set; }
      

        public MessageProperty(string name, string value) : this(name, value, string.Empty)
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