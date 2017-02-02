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
    public class SecurityTokenReferenceProvider : ISecurityTokenReferenceProvider
    {
        private readonly IDictionary<X509ReferenceType, SecurityTokenReference> _references;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityTokenReferenceProvider"/> class. 
        /// Create a new Security Token Reference Provider
        /// with Defaults registered
        /// </summary>
        public SecurityTokenReferenceProvider()
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
            if (HasEnvelopeTag(envelopeDocument, tag: "BinarySecurityToken"))
                return new BinarySecurityTokenReference();

            if (HasEnvelopeTag(envelopeDocument, tag: "X509SerialNumber"))
                return new IssuerSecurityTokenReference();

            if (HasEnvelopeTag(envelopeDocument, tag: "KeyIdentifier"))
                return new KeyIdentifierSecurityTokenReference();

            // Return a BinarySecurityTokenReference as a default when no other option was possible.
            return new BinarySecurityTokenReference();
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