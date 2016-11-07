using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Eu.EDelivery.AS4.Builders.Internal
{
    /// <summary>
    /// Soap Envelope Builder
    /// </summary>
    internal class SoapEnvelopeBuilder
    {
        private static readonly Dictionary<SoapNamespace, string> Prefixes = new Dictionary<SoapNamespace, string>
        {
            {SoapNamespace.Ebms, "eb"},
            {SoapNamespace.Soap, "s12"},
            {SoapNamespace.SecurityUtility, "wsu"},
            {SoapNamespace.SecurityExt, "wsse"}
        };

        private static readonly Dictionary<SoapNamespace, string> Namespaces = new Dictionary<SoapNamespace, string>
        {
            {SoapNamespace.Ebms, Constants.Namespaces.EbmsXmlCore},
            {SoapNamespace.Soap, Constants.Namespaces.Soap12},
            {SoapNamespace.SecurityUtility, Constants.Namespaces.WssSecurityUtility},
            {SoapNamespace.SecurityExt, Constants.Namespaces.WssSecuritySecExt}
        };

        private XmlElement _bodyElement;
        private XmlDocument _document;
        private XmlElement _envelopeElement;
        private XmlElement _headerElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoapEnvelopeBuilder"/> class. 
        /// Create a Soap Envelope Builder
        /// </summary>
        public SoapEnvelopeBuilder()
        {
            InitializeBuilder();
        }

        private void InitializeBuilder()
        {
            this._document = new XmlDocument();

            this._envelopeElement = CreateElement(SoapNamespace.Soap, "Envelope");
            this._bodyElement = CreateElement(SoapNamespace.Soap, "Body");
            this._headerElement = CreateElement(SoapNamespace.Soap, "Header");

            this._document.AppendChild(this._envelopeElement);
        }

        /// <summary>
        /// Set Messaging Header
        /// </summary>
        /// <param name="messagingHeader"></param>
        public SoapEnvelopeBuilder SetMessagingHeader(Xml.Messaging messagingHeader)
        {
            if (messagingHeader == null)
                throw new ArgumentNullException(nameof(messagingHeader));

            if (this._headerElement == null)
                this._headerElement = CreateElement(SoapNamespace.Soap, "Header");

            XmlDocument xmlDocument = SerializeMessagingHeaderToXmlDocument(messagingHeader);
            XmlNode messagingNode = this._document.ImportNode(xmlDocument.DocumentElement, deep: true);

            this._headerElement.AppendChild(messagingNode);
            this._envelopeElement.AppendChild(this._headerElement);

            return this;
        }

        private XmlDocument SerializeMessagingHeaderToXmlDocument(Xml.Messaging messagingHeader)
        {
            var xmlDocument = new XmlDocument();

            using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
            {
                var serializer = new XmlSerializer(typeof(Xml.Messaging));
                serializer.Serialize(writer, messagingHeader, GetXmlNamespaces());
            }

            return xmlDocument;
        }

        private XmlSerializerNamespaces GetXmlNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            foreach (KeyValuePair<SoapNamespace, string> prefix in Prefixes)
                namespaces.Add(prefix.Value, Namespaces[prefix.Key]);

            return namespaces;
        }

        /// <summary>
        /// Set the Security Header Node to the Header Element
        /// </summary>
        /// <param name="securityHeader"></param>
        public SoapEnvelopeBuilder SetSecurityHeader(XmlNode securityHeader)
        {
            if (securityHeader == null)
                throw new ArgumentNullException(nameof(securityHeader));

            if (this._headerElement == null)
                this._headerElement = CreateElement(SoapNamespace.Soap, "Header");

            securityHeader = this._document.ImportNode(securityHeader, deep: true);

            this._headerElement.AppendChild(securityHeader);
            return this;
        }


        /// <summary>
        /// Set the BodyId for the <Body/> Element
        /// </summary>
        /// <param name="bodySecurityId">
        /// </param>
        public SoapEnvelopeBuilder SetMessagingBody(string bodySecurityId)
        {
            if (this._bodyElement == null)
                this._bodyElement = CreateElement(SoapNamespace.Soap, "Body");

            this._bodyElement.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, bodySecurityId);
            XmlNode xmlNode = this._document.ImportNode(this._bodyElement, deep: true);
            this._envelopeElement.AppendChild(xmlNode);

            return this;
        }

        /// <summary>
        /// Build the Soap Envelope
        /// </summary>
        /// <returns></returns>
        public XmlDocument Build()
        {
            return this._document;
        }

        /// <summary>
        /// Break Down the Soap Envelope Builder
        /// </summary>
        /// <returns></returns>
        public SoapEnvelopeBuilder BreakDown()
        {
            InitializeBuilder();
            return this;
        }

        private XmlElement CreateElement(SoapNamespace soapNamespace, string elementName)
        {
            return this._document.CreateElement(Prefixes[soapNamespace], elementName, Namespaces[soapNamespace]);
        }
    }

    internal enum SoapNamespace
    {
        Soap,
        Ebms,
        SecurityUtility,
        SecurityExt
    }
}