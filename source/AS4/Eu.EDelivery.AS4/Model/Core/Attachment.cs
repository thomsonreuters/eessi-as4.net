using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Utilities;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Attachment
    {
        public string Id { get; set; }
        public string ContentType { get; set; }

        [XmlIgnore]
        public Stream Content { get; set; }

        public string Location { get; set; }
        public List<Schema> Schemas { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public Attachment() : this(IdentifierFactory.Instance.Create())
        {
        }

        public Attachment(string id)
        {
            this.Id = id;
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            this.Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.Schemas = new List<Schema>();
            this.ContentType = "application/octet-stream";
        }
    }

    public class Schema
    {
        public string Location { get; set; }
        public string Version { get; set; }
        public string Namespace { get; set; }
    }
}