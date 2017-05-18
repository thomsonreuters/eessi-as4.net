using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
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
            _document = new XmlDocument() { PreserveWhitespace = true };

            _envelopeElement = CreateElement(SoapNamespace.Soap, "Envelope");
            _bodyElement = CreateElement(SoapNamespace.Soap, "Body");
            _headerElement = CreateElement(SoapNamespace.Soap, "Header");

            _document.AppendChild(_envelopeElement);
        }

        /// <summary>
        /// Set Messaging Header
        /// </summary>
        /// <param name="messagingHeader"></param>
        public SoapEnvelopeBuilder SetMessagingHeader(Xml.Messaging messagingHeader)
        {
            if (messagingHeader == null)
                throw new ArgumentNullException(nameof(messagingHeader));

            if (_headerElement == null)
            {
                _headerElement = CreateElement(SoapNamespace.Soap, "Header");
            }

            XmlDocument xmlDocument = SerializeMessagingHeaderToXmlDocument(messagingHeader);
            XmlNode messagingNode = _document.ImportNode(xmlDocument.DocumentElement, deep: true);

            _headerElement.AppendChild(messagingNode);
            _envelopeElement.AppendChild(_headerElement);

            return this;
        }

        private static XmlDocument SerializeMessagingHeaderToXmlDocument(Xml.Messaging messagingHeader)
        {
            var xmlDocument = new XmlDocument() { PreserveWhitespace = true };

            using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
            {
                var serializer = new XmlSerializer(typeof(Xml.Messaging));
                serializer.Serialize(writer, messagingHeader, GetXmlNamespaces());
            }

            return xmlDocument;
        }

        private static XmlSerializerNamespaces GetXmlNamespaces()
        {
            var namespaces = new XmlSerializerNamespaces();
            foreach (KeyValuePair<SoapNamespace, string> prefix in Prefixes)
            {
                namespaces.Add(prefix.Value, Namespaces[prefix.Key]);
            }

            return namespaces;
        }

        /// <summary>
        /// Set the Security Header Node to the Header Element
        /// </summary>
        /// <param name="securityHeader"></param>
        public SoapEnvelopeBuilder SetSecurityHeader(XmlNode securityHeader)
        {
            if (securityHeader == null)
            {
                throw new ArgumentNullException(nameof(securityHeader));
            }

            if (_headerElement == null)
            {
                _headerElement = CreateElement(SoapNamespace.Soap, "Header");
            }

            securityHeader = _document.ImportNode(securityHeader, deep: true);

            _headerElement.AppendChild(securityHeader);
            return this;
        }

        /// <summary>
        /// Set the Routing Input tag Node to the Envelope
        /// </summary>
        /// <param name="routingInput"></param>
        /// <returns></returns>
        public SoapEnvelopeBuilder SetRoutingInput(RoutingInput routingInput)
        {
            var xmlDocument = new XmlDocument() { PreserveWhitespace = true };

            using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
            {
                var serializer = new XmlSerializer(typeof(Xml.RoutingInput));
                serializer.Serialize(writer, routingInput, GetXmlNamespaces());
            }

            XmlNode routingInputNode = _document.ImportNode(xmlDocument.DocumentElement, deep: true);
            _headerElement.AppendChild(routingInputNode);

            return this;
        }

        /// <summary>
        /// Set the BodyId for the <Body/> Element
        /// </summary>
        /// <param name="bodySecurityId">
        /// </param>
        public SoapEnvelopeBuilder SetMessagingBody(string bodySecurityId)
        {
            if (_bodyElement == null)
            {
                _bodyElement = CreateElement(SoapNamespace.Soap, "Body");
            }

            _bodyElement.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, bodySecurityId);
            XmlNode xmlNode = _document.ImportNode(_bodyElement, deep: true);

            _envelopeElement.AppendChild(xmlNode);
            return this;
        }

        /// <summary>
        /// Set the To Node to the Envelope
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public SoapEnvelopeBuilder SetToHeader(To to)
        {
            XmlNode toNode = _document.CreateElement("wsa", "To", Constants.Namespaces.Addressing);
            toNode.InnerText = Constants.Namespaces.ICloud;

            XmlAttribute roleAttribute = _document.CreateAttribute(Prefixes[SoapNamespace.Soap], "role", Namespaces[SoapNamespace.Soap]);
            roleAttribute.Value = to.Role;
            toNode.Attributes.Append(roleAttribute);

            _headerElement.AppendChild(toNode);
            return this;
        }

        /// <summary>
        /// Set the Action Node to the Envelope
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public SoapEnvelopeBuilder SetActionHeader(string action)
        {
            XmlNode actionNode = _document.CreateElement("wsa", "Action", Constants.Namespaces.Addressing);
            actionNode.InnerText = action;

            _headerElement.AppendChild(actionNode);
            return this;
        }

        private XmlElement CreateElement(SoapNamespace soapNamespace, string elementName)
        {
            return _document.CreateElement(Prefixes[soapNamespace], elementName, Namespaces[soapNamespace]);
        }

        /// <summary>
        /// Build the Soap Envelope
        /// </summary>
        /// <returns></returns>
        public XmlDocument Build()
        {
            return _document;
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