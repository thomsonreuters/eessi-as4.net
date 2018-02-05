using System;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.References;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// Wrapper for specific Key Encryption Configuration
    /// </summary>
    public class KeyEncryptionConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyEncryptionConfiguration" /> class.
        /// </summary>
        /// <param name="tokenReference">The token reference.</param>
        /// <param name="keyEncryption">The key encryption.</param>
        public KeyEncryptionConfiguration(SecurityTokenReference tokenReference, KeyEncryption keyEncryption)
        {
            if (tokenReference == null)
            {
                throw new ArgumentNullException(nameof(tokenReference));
            }

            SecurityTokenReference = tokenReference;
            EncryptionMethod = keyEncryption.TransportAlgorithm;
            DigestMethod = keyEncryption.DigestAlgorithm;
            Mgf = keyEncryption.MgfAlgorithm;
        }

        /// <summary>
        /// Gets the encryption method.
        /// </summary>
        /// <value>The encryption method.</value>
        public string EncryptionMethod { get; private set; }

        /// <summary>
        /// Gets the digest method.
        /// </summary>
        /// <value>The digest method.</value>
        public string DigestMethod { get; private set; }

        /// <summary>
        /// Gets the MGF.
        /// </summary>
        /// <value>The MGF.</value>
        public string Mgf { get; private set; }

        /// <summary>
        /// Gets or sets the security token reference.
        /// </summary>
        /// <value>The security token reference.</value>
        public SecurityTokenReference SecurityTokenReference { get; set; }
    }
}