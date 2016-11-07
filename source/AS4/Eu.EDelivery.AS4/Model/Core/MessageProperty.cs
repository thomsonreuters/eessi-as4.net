namespace Eu.EDelivery.AS4.Model.Core
{
    public class MessageProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public MessageProperty() {}

        public MessageProperty(string name, string type, string value)
        {
            this.Name = name;
            this.Type = type;
            this.Value = value;
        }
    }
}