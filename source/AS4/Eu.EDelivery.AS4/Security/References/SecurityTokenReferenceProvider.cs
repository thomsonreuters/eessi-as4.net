using System;
using System.Xml;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Security.References
{

    internal class SecurityTokenReferenceProvider : ISecurityTokenReferenceProvider
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

        public SecurityTokenReference Get(XmlElement envelopeDocument, SecurityTokenType type)
        {
            string xpathQuery = "";

            if (type == SecurityTokenType.Signing)
            {
                xpathQuery = "//*[local-name()='{0}']";
            }
            else if (type == SecurityTokenType.Encryption)
            {
                xpathQuery = ".//*[local-name()='{0}']";
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (HasEnvelopeTag(envelopeDocument, xpathSelector: String.Format(xpathQuery, "BinarySecurityToken")))
            {
                return new BinarySecurityTokenReference(envelopeDocument);
            }

            if (HasEnvelopeTag(envelopeDocument, xpathSelector: String.Format(xpathQuery, "X509SerialNumber")))
            {
                return new IssuerSecurityTokenReference(envelopeDocument, _certificateRepository);
            }

            if (HasEnvelopeTag(envelopeDocument, xpathSelector: String.Format(xpathQuery, "KeyIdentifier")))
            {
                return new KeyIdentifierSecurityTokenReference(envelopeDocument, _certificateRepository);
            }

            return new BinarySecurityTokenReference(envelopeDocument);
        }

        private static bool HasEnvelopeTag(XmlNode element, string xpathSelector)
        {
            return element?.SelectSingleNode(xpathSelector) != null;
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
        SecurityTokenReference Get(XmlElement envelopeDocument, SecurityTokenType type);
    }
}