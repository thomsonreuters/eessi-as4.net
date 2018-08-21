using System;
using System.Collections.Generic;
using System.Linq;
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
            private static readonly Dictionary<SoapNamespace, XmlQualifiedName> NamespaceInformation
                = new Dictionary<SoapNamespace, XmlQualifiedName>()
                {
                    { SoapNamespace.Ebms, new XmlQualifiedName("eb", Constants.Namespaces.EbmsXmlCore) },
                    { SoapNamespace.Soap, new XmlQualifiedName("s12", Constants.Namespaces.Soap12) },
                    { SoapNamespace.SecurityUtility, new XmlQualifiedName("wsu", Constants.Namespaces.WssSecurityUtility) },
                    { SoapNamespace.SecurityExt, new XmlQualifiedName("wsse", Constants.Namespaces.WssSecuritySecExt) }
                };

            private static readonly XmlSerializerNamespaces XmlSerializerNamespaceInfo
                = new XmlSerializerNamespaces(NamespaceInformation.Values.ToArray());

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
                    _document = new XmlDocument { PreserveWhitespace = true };

                    _envelopeElement = CreateElement(SoapNamespace.Soap, "Envelope");
                    _bodyElement = CreateElement(SoapNamespace.Soap, "Body");
                    _headerElement = CreateElement(SoapNamespace.Soap, "Header");

                    _document.AppendChild(_envelopeElement);
                }
                else
                {
                    var soapNamespace = NamespaceInformation[SoapNamespace.Soap];

                    var nsMgr = new XmlNamespaceManager(envelopeDocument.NameTable);
                    nsMgr.AddNamespace(soapNamespace.Name, soapNamespace.Namespace);

                    _document = envelopeDocument;

                    _envelopeElement = envelopeDocument.SelectSingleNode($"/{soapNamespace.Name}:Envelope", nsMgr) as XmlElement;
                    _headerElement = envelopeDocument.SelectSingleNode($"/{soapNamespace.Name}:Envelope/{soapNamespace.Name}:Header", nsMgr) as XmlElement;
                    _bodyElement = envelopeDocument.SelectSingleNode($"/{soapNamespace.Name}:Envelope/{soapNamespace.Name}:Body", nsMgr) as XmlElement;
                }

                if (_envelopeElement == null)
                {
                    throw new NotSupportedException(
                        $"Envelope document requires a root <s12:Envelope/> element where s12={Constants.Namespaces.Soap12}");
                }
            }

            /// <summary>
            /// Set Messaging Header
            /// </summary>
            /// <param name="messagingHeader"></param>
            public SoapEnvelopeBuilder SetMessagingHeader(Messaging messagingHeader)
            {
                if (messagingHeader == null)
                {
                    throw new ArgumentNullException(nameof(messagingHeader));
                }

                _messagingHeaderElement = SerializeMessagingHeaderToXmlDocument(messagingHeader);

                return this;
            }

            private XmlNode SerializeMessagingHeaderToXmlDocument(Messaging messagingHeader)
            {
                var xmlDocument = new XmlDocument { PreserveWhitespace = true };

                using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
                {
                    var serializer = new XmlSerializer(typeof(Messaging));
                    serializer.Serialize(writer, messagingHeader, XmlSerializerNamespaceInfo);
                }

                return _document.ImportNode(xmlDocument.DocumentElement, deep: true);
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
                var xmlDocument = new XmlDocument { PreserveWhitespace = true };

                using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
                {
                    var serializer = new XmlSerializer(typeof(RoutingInput));
                    serializer.Serialize(writer, routingInput, XmlSerializerNamespaceInfo);
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
                _bodyElement?.SetAttribute("Id", Constants.Namespaces.WssSecurityUtility, bodySecurityId);

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

                var soapNamespace = NamespaceInformation[SoapNamespace.Soap];

                XmlAttribute roleAttribute = _document.CreateAttribute(soapNamespace.Name, "role", soapNamespace.Namespace);
                roleAttribute.Value = to.Role;
                toNode.Attributes?.Append(roleAttribute);

                _headerElement?.AppendChild(toNode);
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

                _headerElement?.AppendChild(actionNode);
                return this;
            }

            private XmlElement CreateElement(SoapNamespace namespaceInfo, string elementName)
            {
                return _document.CreateElement(
                    prefix: NamespaceInformation[namespaceInfo].Name,
                    localName: elementName,
                    namespaceURI: NamespaceInformation[namespaceInfo].Namespace);
            }

            /// <summary>
            /// Build the Soap Envelope
            /// </summary>
            /// <returns></returns>
            public XmlDocument Build()
            {
                var nsMgr = new XmlNamespaceManager(_document.NameTable);

                nsMgr.AddNamespace("soap", NamespaceInformation[SoapNamespace.Soap].Namespace);
                nsMgr.AddNamespace("wsse", NamespaceInformation[SoapNamespace.SecurityExt].Namespace);

                if (_securityHeaderElement != null && _headerElement != null)
                {
                    var existingSecurityHeader = _headerElement.SelectSingleNode("//soap:Header/wsse:Security", nsMgr);
                    if (existingSecurityHeader != null)
                    {
                        _headerElement.ReplaceChild(_securityHeaderElement, existingSecurityHeader);
                    }
                    else
                    {
                        _headerElement.InsertBefore(_securityHeaderElement, _headerElement.FirstChild);
                    }
                }

                if (_routingInputHeaderElement != null)
                {
                    _headerElement?.AppendChild(_routingInputHeaderElement);
                }

                if (_messagingHeaderElement != null)
                {
                    _headerElement?.AppendChild(_messagingHeaderElement);
                }

                if (_headerElement?.HasChildNodes == true)
                {
                    var existingHeader = _envelopeElement.SelectSingleNode("/soap:Envelope/soap:Header", nsMgr);

                    if (existingHeader != null)
                    {
                        _envelopeElement.ReplaceChild(_headerElement, existingHeader);
                    }
                    else
                    {
                        _envelopeElement.AppendChild(_headerElement);
                    }
                }

                if (_bodyElement?.HasAttributes == true)
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