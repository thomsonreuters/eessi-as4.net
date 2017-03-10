using System;

namespace Eu.EDelivery.AS4
{
    public class InfoAttribute : Attribute
    {
        public string FriendlyName { get; private set; }
        public string Type { get; private set; }
        public string Regex { get; private set; }

        public InfoAttribute(string friendlyName)
        {
            FriendlyName = friendlyName;
        }

        public InfoAttribute(string friendlyName, string regex)
        {
            FriendlyName = friendlyName;
            Regex = regex;
        }

        public InfoAttribute(string friendlyName, string regex, string type)
        {
            FriendlyName = friendlyName;
            Regex = regex;
            Type = type;
        }
    }
}