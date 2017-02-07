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
            this._certificateRepository = certificateRepository;
        }
        
        public SecurityTokenReference Get(X509ReferenceType referenceType)
        {
            switch (referenceType)
            {
                case X509ReferenceType.BSTReference:
                    return new BinarySecurityTokenReference();
                case X509ReferenceType.IssuerSerial:
                    return new IssuerSecurityTokenReference(this._certificateRepository);
                case X509ReferenceType.KeyIdentifier:
                    return new KeyIdentifierSecurityTokenReference(this._certificateRepository);

                default:
                    throw new NotSupportedException($"There exists no SecurityTokenReferenceType for {referenceType}");
            }
        }

        public SecurityTokenReference Get(XmlElement envelopeDocument)
        {
            if (HasEnvelopeTag(envelopeDocument, tag: "BinarySecurityToken"))
            {
                return new BinarySecurityTokenReference(envelopeDocument);
            }

            if (HasEnvelopeTag(envelopeDocument, tag: "X509SerialNumber"))
            {
                return new IssuerSecurityTokenReference(envelopeDocument, this._certificateRepository);
            }

            if (HasEnvelopeTag(envelopeDocument, tag: "KeyIdentifier"))
            {
                return new KeyIdentifierSecurityTokenReference(envelopeDocument, this._certificateRepository);
            }

            throw new NotSupportedException("No matching SecurityTokenReference implementation found");
        }

        private static bool HasEnvelopeTag(XmlNode element, string tag)
        {
            return element?.SelectSingleNode($"//*[local-name()='{tag}']") != null;
        }
    }

    /// <summary>
    /// Interface to declare the selection of 
    /// <see cref="SecurityTokenReference"/> implementations
    /// </summary>
    public interface ISecurityTokenReferenceProvider
    {
        SecurityTokenReference Get(X509ReferenceType referenceType);
        SecurityTokenReference Get(XmlElement envelopeDocument);
    }
}