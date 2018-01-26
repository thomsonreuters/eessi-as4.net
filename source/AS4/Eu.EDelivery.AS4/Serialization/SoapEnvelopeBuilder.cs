using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Xml;

namespace Eu.EDelivery.AS4.Serialization
{
    partial class SoapEnvelopeSerializer
    {
        /// <summary>
        /// Soap Envelope Builder
        /// </summary>
        internal class SoapEnvelopeBuilder
        {
            // TODO: refactor this to a simpler structure.

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

            private readonly XmlElement _bodyElement;
            private readonly XmlDocument _document;
            private readonly XmlElement _envelopeElement;
            private readonly XmlElement _headerElement;

            private XmlNode _securityHeaderElement;
            private XmlNode _messagingHeaderElement;
            private XmlNode _routingInputHeaderElement;

            /// <summary>
            /// Initializes a new instance of the <see cref="SoapEnvelopeBuilder"/> class. 
            /// Create a Soap Envelope Builder
            /// </summary>
            public SoapEnvelopeBuilder() : this(null) { }

            public SoapEnvelopeBuilder(XmlDocument envelopeDocument)
            {
                if (envelopeDocument == null)
                {
                    _document = new XmlDocument() { PreserveWhitespace = true };

                    _envelopeElement = CreateElement(SoapNamespace.Soap, "Envelope");
                    _bodyElement = CreateElement(SoapNamespace.Soap, "Body");
                    _headerElement = CreateElement(SoapNamespace.Soap, "Header");

                    _document.AppendChild(_envelopeElement);
                }
                else
                {
                    var nsMgr = new XmlNamespaceManager(envelopeDocument.NameTable);
                    nsMgr.AddNamespace("s", Namespaces[SoapNamespace.Soap]);
                    _document = envelopeDocument;

                    _envelopeElement = envelopeDocument.SelectSingleNode("/s:Envelope", nsMgr) as XmlElement;
                    _headerElement = envelopeDocument.SelectSingleNode($"/s:Envelope/s:Header", nsMgr) as XmlElement;
                    _bodyElement = envelopeDocument.SelectSingleNode("/s:Envelope/s:Body", nsMgr) as XmlElement;
                }
            }

            /// <summary>
            /// Set Messaging Header
            /// </summary>
            /// <param name="messagingHeader"></param>
            public SoapEnvelopeBuilder SetMessagingHeader(Xml.Messaging messagingHeader)
            {
                if (messagingHeader == null)
                {
                    throw new ArgumentNullException(nameof(messagingHeader));
                }

                _messagingHeaderElement = SerializeMessagingHeaderToXmlDocument(messagingHeader);

                return this;
            }

            private XmlNode SerializeMessagingHeaderToXmlDocument(Xml.Messaging messagingHeader)
            {
                var xmlDocument = new XmlDocument() { PreserveWhitespace = true };

                using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
                {
                    var serializer = new XmlSerializer(typeof(Xml.Messaging));
                    serializer.Serialize(writer, messagingHeader, GetXmlNamespaces());
                }

                return _document.ImportNode(xmlDocument.DocumentElement, deep: true);
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

                _securityHeaderElement = _document.ImportNode(securityHeader, deep: true);

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

                _routingInputHeaderElement = _document.ImportNode(xmlDocument.DocumentElement, deep: true);

                return this;
            }

            /// <summary>
            /// Set the BodyId for the <Body/> Element
            /// </summary>
            /// <param name="bodySecurityId">
            /// </param>
            public SoapEnvelopeBuilder SetMessagingBody(string bodySecurityId)
            {
                _bodyElement.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, bodySecurityId);

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
                if (_securityHeaderElement != null)
                {
                    var existingSecurityHeader = _headerElement.SelectSingleNode("//*[local-name()='Security']");
                    if (existingSecurityHeader != null)
                    {
                        _headerElement.ReplaceChild(_securityHeaderElement, existingSecurityHeader);
                    }
                    else
                    {
                        _headerElement.AppendChild(_securityHeaderElement);
                    }
                }

                if (_routingInputHeaderElement != null)
                {
                    _headerElement.AppendChild(_routingInputHeaderElement);
                }

                if (_messagingHeaderElement != null)
                {
                    _headerElement.AppendChild(_messagingHeaderElement);
                }

                if (_headerElement.HasChildNodes)
                {
                    _envelopeElement.AppendChild(_headerElement);
                }

                if (_bodyElement.HasAttributes)
                {
                    _envelopeElement.AppendChild(_bodyElement);
                }

                return _document;
            }

            private enum SoapNamespace
            {
                Soap,
                Ebms,
                SecurityUtility,
                SecurityExt
            }
        }
    }
}