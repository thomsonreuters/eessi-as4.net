using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// Wrapper for specific Key Encryption Configuration
    /// </summary>
    public class KeyEncryptionConfiguration
    {
        public string EncryptionMethod { get; private set; } 
        public string DigestMethod { get; private set; } 
        public string Mgf { get; private set; } 
        
        public SecurityTokenReference SecurityTokenReference { get; set; } 

        public KeyEncryptionConfiguration(SecurityTokenReference tokenReference, string encryptionMethod, string digestMethod, string mgf)
        {
            if (tokenReference == null)
            {
                tokenReference = new BinarySecurityTokenReference();
            }

            this.SecurityTokenReference = tokenReference;
            this.EncryptionMethod = encryptionMethod;
            this.DigestMethod = digestMethod;
            this.Mgf = mgf;
        }
    }
}