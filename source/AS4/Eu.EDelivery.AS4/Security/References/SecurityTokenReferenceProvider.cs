using System;
using System.Collections.Generic;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Class to provide <see cref="SecurityTokenReference"/> implementations
    /// TODO: will be moved to <see cref="SecurityTokenReference"/>
    /// </summary>
    public class OldSecurityTokenReferenceProvider : ISecurityTokenReferenceProvider
    {
        private readonly IDictionary<X509ReferenceType, SecurityTokenReference> _references;

        /// <summary>
        /// Initializes a new instance of the <see cref="OldSecurityTokenReferenceProvider"/> class. 
        /// Create a new Security Token Reference Provider
        /// with Defaults registered
        /// </summary>
        public OldSecurityTokenReferenceProvider()
        {
            this._references = new Dictionary<X509ReferenceType, SecurityTokenReference>
            {
                [X509ReferenceType.BSTReference] = new BinarySecurityTokenReference(),
                [X509ReferenceType.IssuerSerial] = new IssuerSecurityTokenReference(),
                [X509ReferenceType.KeyIdentifier] = new KeyIdentifierSecurityTokenReference()
            };
        }

        /// <summary>
        /// Get the <see cref="SecurityTokenReference"/> implementation
        /// based on a <see cref="X509ReferenceType"/>
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        public SecurityTokenReference Get(X509ReferenceType referenceType)
        {
            return this._references.ReadMandatoryProperty(referenceType);
        }

        /// <summary>
        /// Get the <see cref="SecurityTokenReference"/> implementation
        /// based on a <see cref=""/>
        /// </summary>
        /// <param name="envelopeDocument"></param>
        /// <returns></returns>
        public SecurityTokenReference Get(XmlNode envelopeDocument)
        {
            var binaryEnvelope = GetElement(envelopeDocument, tag: "BinarySecurityToken");
            if (binaryEnvelope != null)
            {
                return new BinarySecurityTokenReference(binaryEnvelope);
            }
            else
            {
                var issuerEnvelope = GetElement(envelopeDocument, tag: "X509SerialNumber");

                if (issuerEnvelope != null)
                {
                    return new IssuerSecurityTokenReference(issuerEnvelope);
                }
                else
                {
                    var keyEnvelope = GetElement(envelopeDocument, tag: "KeyIdentifier");

                    if (keyEnvelope != null)
                    {
                        return new KeyIdentifierSecurityTokenReference(keyEnvelope);
                    }
                }
            }

            throw new NotSupportedException("No matching SecurityTokenReference implementation found");

            ////if (HasEnvelopeTag(envelopeDocument, tag: "BinarySecurityToken"))
            ////    return new BinarySecurityTokenReference();

            ////if (HasEnvelopeTag(envelopeDocument, tag: "X509SerialNumber"))
            ////    return new IssuerSecurityTokenReference();

            ////if (HasEnvelopeTag(envelopeDocument, tag: "KeyIdentifier"))
            ////    return new KeyIdentifierSecurityTokenReference();

            ////// Return a BinarySecurityTokenReference as a default when no other option was possible.
            ////return new BinarySecurityTokenReference();
        }

        private static XmlElement GetElement(XmlNode envelope, string tag)
        {
            return envelope?.SelectSingleNode($"//*[local-name()='{tag}']") as XmlElement;            
        }

        ////private static bool HasEnvelopeTag(XmlNode envelope, string tag)
        ////{
        ////    return envelope?.SelectSingleNode($"//*[local-name()='{tag}']") != null;
        ////}
    }

    public class SecurityTokenReferenceProvider : ISecurityTokenReferenceProvider
    {
        public SecurityTokenReference Get(X509ReferenceType referenceType)
        {
            switch (referenceType)
            {
                case X509ReferenceType.BSTReference:
                    return new BinarySecurityTokenReference();
                case X509ReferenceType.IssuerSerial:
                    return new IssuerSecurityTokenReference();
                case X509ReferenceType.KeyIdentifier:
                    return new KeyIdentifierSecurityTokenReference();

                default:
                    throw new NotSupportedException($"There exists no SecurityTokenReferenceType for {referenceType}");
            }
        }

        public SecurityTokenReference Get(XmlNode envelopeDocument)
        {
            if (HasEnvelopeTag(envelopeDocument, tag: "BinarySecurityToken"))
                return new BinarySecurityTokenReference();

            if (HasEnvelopeTag(envelopeDocument, tag: "X509SerialNumber"))
                return new IssuerSecurityTokenReference();

            if (HasEnvelopeTag(envelopeDocument, tag: "KeyIdentifier"))
                return new KeyIdentifierSecurityTokenReference();

            // Return a BinarySecurityTokenReference as a default when no other option was possible.
            throw new NotSupportedException("No matching TokenReferenceType found in envelopeDocument");
            //return new BinarySecurityTokenReference();
        }

        private static bool HasEnvelopeTag(XmlNode envelope, string tag)
        {
            return envelope?.SelectSingleNode($"//*[local-name()='{tag}']") != null;
        }
    }

    /// <summary>
    /// Interface to declare the selection of 
    /// <see cref="SecurityTokenReference"/> implementations
    /// </summary>
    public interface ISecurityTokenReferenceProvider
    {
        SecurityTokenReference Get(X509ReferenceType referenceType);
        SecurityTokenReference Get(XmlNode envelopeDocument);
    }
}