using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Eu.EDelivery.AS4.Xml;

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
        /// Set the Routing Input tag Node to the Envelope
        /// </summary>
        /// <param name="routingInput"></param>
        /// <returns></returns>
        public SoapEnvelopeBuilder SetRoutingInput(RoutingInput routingInput)
        {
            var xmlDocument = new XmlDocument();

            using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
            {
                var serializer = new XmlSerializer(typeof(Xml.RoutingInput));
                serializer.Serialize(writer, routingInput, GetXmlNamespaces());
            }

            XmlNode routingInputNode = this._document.ImportNode(xmlDocument.DocumentElement, deep: true);
            this._headerElement.AppendChild(routingInputNode);

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

        /// <summary>
        /// Set the To Node to the Envelope
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public SoapEnvelopeBuilder SetToHeader(To to)
        {
            XmlNode toNode = this._document.CreateElement("wsa", "To", Constants.Namespaces.Addressing);
            toNode.InnerText = Constants.Namespaces.ICloud;

            XmlAttribute roleAttribute = this._document.CreateAttribute("s12", "role", Constants.Namespaces.Soap12);
            roleAttribute.Value = to.Role;
            toNode.Attributes.Append(roleAttribute);

            this._headerElement.AppendChild(toNode);
            return this;
        }

        /// <summary>
        /// Set the Action Node to the Envelope
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public SoapEnvelopeBuilder SetActionHeader(string action)
        {
            XmlNode actionNode = this._document.CreateElement("wsa", "Action", Constants.Namespaces.Addressing);
            actionNode.InnerText = action;

            this._headerElement.AppendChild(actionNode);
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
    }

    internal enum SoapNamespace
    {
        Soap,
        Ebms,
        SecurityUtility,
        SecurityExt
    }
}