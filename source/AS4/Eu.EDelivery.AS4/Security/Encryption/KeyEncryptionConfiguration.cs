using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// Wrapper for specific Key Encryption Configuration
    /// </summary>
    internal class KeyEncryptionConfiguration
    {
        public string EncryptionMethod { get; set; } = EncryptedXml.XmlEncRSAOAEPUrl;
        public string DigestMethod { get; set; } = EncryptionStrategy.XmlEncSHA1Url;

        // TODO: load/select the right Security Token Reference
        public SecurityTokenReference SecurityTokenReference { get; set; } = new BinarySecurityTokenReference();
    }
}