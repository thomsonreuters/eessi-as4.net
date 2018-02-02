using System;
using System.Xml;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Security.References
{

    public class SecurityTokenReferenceProvider : ISecurityTokenReferenceProvider
    {
        private readonly ICertificateRepository _certificateRepository;

        public SecurityTokenReferenceProvider(ICertificateRepository certificateRepository)
        {
            _certificateRepository = certificateRepository;
        }

        public SecurityTokenReference Get(X509ReferenceType referenceType)
        {
            switch (referenceType)
            {
                case X509ReferenceType.BSTReference:
                    return new BinarySecurityTokenReference();
                case X509ReferenceType.IssuerSerial:
                    return new IssuerSecurityTokenReference(_certificateRepository);
                case X509ReferenceType.KeyIdentifier:
                    return new KeyIdentifierSecurityTokenReference(_certificateRepository);

                default:
                    return new BinarySecurityTokenReference();
            }
        }

        public SecurityTokenReference Get(XmlDocument envelopeDocument, SecurityTokenType type)
        {
            XmlElement keyInfoElement;

            if (type == SecurityTokenType.Signing)
            {
                keyInfoElement =
                    envelopeDocument.SelectSingleNode(
                        @"//*[local-name()='Header']/*[local-name()='Security']/*[local-name()='Signature']/*[local-name()='KeyInfo']/*[local-name()='SecurityTokenReference']") as XmlElement;
            }
            else if (type == SecurityTokenType.Encryption)
            {
                keyInfoElement =
                    envelopeDocument.SelectSingleNode(
                        @"//*[local-name()='Header']/*[local-name()='Security']/*[local-name()='EncryptedKey']/*[local-name()='KeyInfo']/*[local-name()='SecurityTokenReference']") as XmlElement;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (keyInfoElement == null)
            {
                return null; // ? TODO: what should we do here ?
            }

            if (HasEnvelopeTag(keyInfoElement, securityTokenNodeName: "Reference"))
            {
                return new BinarySecurityTokenReference(keyInfoElement);
            }

            if (HasEnvelopeTag(keyInfoElement, securityTokenNodeName: "KeyIdentifier"))
            {
                return new KeyIdentifierSecurityTokenReference(keyInfoElement, _certificateRepository);
            }

            if (HasEnvelopeTag(keyInfoElement, securityTokenNodeName: "X509Data"))
            {
                return new IssuerSecurityTokenReference(keyInfoElement, _certificateRepository);
            }

            throw new NotSupportedException("Unable to retrieve SecurityTokenReference of type " + keyInfoElement.OuterXml);
        }

        private static bool HasEnvelopeTag(XmlNode element, string securityTokenNodeName)
        {
            return element?.SelectSingleNode($"./*[local-name()='{securityTokenNodeName}']") != null;
        }
    }

    public enum SecurityTokenType
    {
        Signing,
        Encryption
    }

    /// <summary>
    /// Interface to declare the selection of 
    /// <see cref="SecurityTokenReference"/> implementations
    /// </summary>
    public interface ISecurityTokenReferenceProvider
    {
        SecurityTokenReference Get(X509ReferenceType referenceType);
        SecurityTokenReference Get(XmlDocument envelopeDocument, SecurityTokenType type);
    }
}