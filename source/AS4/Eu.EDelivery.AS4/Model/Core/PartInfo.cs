using System.Collections.Generic;
using System.Diagnostics;

namespace Eu.EDelivery.AS4.Model.Core
{
    [DebuggerDisplay("Href {Href}")]
    public class PartInfo
    {
        private string _href;

        public string Href
        {
            get => _href;
            set => _href = value?.Replace(" ", string.Empty);
        }

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