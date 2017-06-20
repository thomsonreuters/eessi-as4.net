using System.Collections.Generic;
using System.Diagnostics;

namespace Eu.EDelivery.AS4.Model.Core
{
    [DebuggerDisplay("Href {Href}")]
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