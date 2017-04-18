using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class PartInfo
    {
        public string Href { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public List<Schema> Schemas { get; set; }

        public PartInfo()
        {
            Properties = new Dictionary<string, string>();
        }

        public PartInfo(string href)
        {
            Href = href;
            Properties = new Dictionary<string, string>();
            Schemas = new List<Schema>();
        }
    }
}