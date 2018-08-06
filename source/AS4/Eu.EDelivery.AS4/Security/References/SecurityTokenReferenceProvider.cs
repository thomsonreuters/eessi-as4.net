using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Security.References
{
    internal static class SecurityTokenReferenceProvider
    {
        public static SecurityTokenReference Create(X509Certificate2 certificate, X509ReferenceType referenceType)
        {
            switch (referenceType)
            {
                case X509ReferenceType.BSTReference:
                    return new BinarySecurityTokenReference(certificate);

                case X509ReferenceType.IssuerSerial:
                    return new IssuerSecurityTokenReference(certificate);

                case X509ReferenceType.KeyIdentifier:
                    return new KeyIdentifierSecurityTokenReference(certificate);

                default:
                    return new BinarySecurityTokenReference(certificate);
            }
        }

        public static SecurityTokenReference Get(XmlDocument envelopeDocument, SecurityTokenType type, ICertificateRepository certificateRepository)
        {
            XmlElement keyInfoElement;

            switch (type)
            {
                case SecurityTokenType.Signing:
                    keyInfoElement =
                        envelopeDocument.SelectSingleNode(
                            @"//*[local-name()='Header']/*[local-name()='Security']/*[local-name()='Signature']/*[local-name()='KeyInfo']/*[local-name()='SecurityTokenReference']") as XmlElement;
                    break;

                case SecurityTokenType.Encryption:
                    keyInfoElement =
                        envelopeDocument.SelectSingleNode(
                            @"//*[local-name()='Header']/*[local-name()='Security']/*[local-name()='EncryptedKey']/*[local-name()='KeyInfo']/*[local-name()='SecurityTokenReference']") as XmlElement;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (keyInfoElement == null)
            {
                return null;
            }

            if (HasEnvelopeTag(keyInfoElement, securityTokenNodeName: "Reference"))
            {
                return new BinarySecurityTokenReference(keyInfoElement);
            }

            if (HasEnvelopeTag(keyInfoElement, securityTokenNodeName: "KeyIdentifier"))
            {
                return new KeyIdentifierSecurityTokenReference(keyInfoElement, certificateRepository);
            }

            if (HasEnvelopeTag(keyInfoElement, securityTokenNodeName: "X509Data"))
            {
                return new IssuerSecurityTokenReference(keyInfoElement, certificateRepository);
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
}