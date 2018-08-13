using System;
using System.Security.Cryptography.X509Certificates;
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
        /// <param name="encryptionCertificate">The certificate that must be used to encrypt the symmetric key.</param>
        public KeyEncryptionConfiguration(X509Certificate2 encryptionCertificate)
            : this(encryptionCertificate, KeyEncryption.Default)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyEncryptionConfiguration"/> class.
        /// </summary>
        /// <param name="encryptionCertificate">The certificate that must be used to encrypt the symmetric key.</param>
        /// <param name="keyEncryption">An instance of the <see cref="KeyEncryption"/> class that defines the algorithms that must be used for encryption</param>
        public KeyEncryptionConfiguration(X509Certificate2 encryptionCertificate,
                                          KeyEncryption keyEncryption)
        {
            if (encryptionCertificate == null)
            {
                throw new ArgumentNullException(nameof(encryptionCertificate));
            }

            if (keyEncryption == null)
            {
                throw new ArgumentNullException(nameof(keyEncryption));
            }

            EncryptionCertificate = encryptionCertificate;

            // TODO: this is now hardcoded but should be configurable via the 
            //       (Sending) PMode.
            _securityTokenReferenceType = X509ReferenceType.BSTReference;

            EncryptionMethod = keyEncryption.TransportAlgorithm;
            DigestMethod = keyEncryption.DigestAlgorithm;
            Mgf = keyEncryption.MgfAlgorithm;
        }

        private readonly X509ReferenceType _securityTokenReferenceType;

        public X509Certificate2 EncryptionCertificate { get; }

        /// <summary>
        /// Gets the encryption method.
        /// </summary>
        /// <value>The encryption method.</value>
        public string EncryptionMethod { get; }

        /// <summary>
        /// Gets the digest method.
        /// </summary>
        /// <value>The digest method.</value>
        public string DigestMethod { get; }

        /// <summary>
        /// Gets the MGF.
        /// </summary>
        /// <value>The MGF.</value>
        public string Mgf { get; }

        /// <summary>
        /// Creates the <see cref="SecurityTokenReference"/> instance that must be used.
        /// </summary>
        /// <returns></returns>
        public SecurityTokenReference BuildSecurityTokenReference()
        {
            return SecurityTokenReferenceProvider.Create(EncryptionCertificate, _securityTokenReferenceType);
        }
    }
}