using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Factories;
using MimeKit.IO;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Attachment
    {
        public string Id { get; set; }
        public string ContentType { get; set; }

        private Stream _content;

        [XmlIgnore]
        public Stream Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (ReferenceEquals(_content, value) == false)
                {
                    _content?.Dispose();
                }
                _content = value;
            }
        }

        public string Location { get; set; }
        public List<Schema> Schemas { get; set; }
        public IDictionary<string, string> Properties { get; set; }

        public Attachment() : this(IdentifierFactory.Instance.Create())
        {
        }

        public Attachment(string id)
        {
            Id = id;
            InitializeDefaults();
        }

        public void ResetContentPosition()
        {
            if (Content != null)
            {
                Stream streamToWorkOn = Content;

                if (Content is NonCloseableStream)
                {
                    streamToWorkOn = ((NonCloseableStream)Content).InnerStream;
                }
                else if (Content is FilteredStream)
                {
                    streamToWorkOn = ((FilteredStream)Content).Source;
                }

                if (streamToWorkOn.CanSeek && streamToWorkOn.Position != 0)
                {
                    streamToWorkOn.Position = 0;
                }
            }
        }

        private void InitializeDefaults()
        {
            Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Schemas = new List<Schema>();
            ContentType = "application/octet-stream";
        }
    }

    public class Schema
    {
        public string Location { get; set; }
        public string Version { get; set; }
        public string Namespace { get; set; }
    }
}