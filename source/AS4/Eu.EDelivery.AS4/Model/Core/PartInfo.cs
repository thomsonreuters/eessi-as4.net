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
            this.Properties = new Dictionary<string, string>();
        }

        public PartInfo(string href)
        {
            this.Href = href;
            this.Properties = new Dictionary<string, string>();
            this.Schemas = new List<Schema>();
        }
    }
}