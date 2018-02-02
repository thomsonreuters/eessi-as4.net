using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// WS Security Signed Xml
    /// </summary>
    public class SecurityHeader
    {
        private ISigningStrategy _signingStrategy;
        private IEncryptionStrategy _encryptionStrategy;

        public bool IsSigned => _signingStrategy != null;
        public bool IsEncrypted { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityHeader"/> class. 
        /// Create empty <see cref="SecurityHeader"/>
        /// </summary>
        public SecurityHeader() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityHeader"/> class. 
        /// </summary>
        /// <param name="signingStrategy">
        /// </param>
        /// <param name="isEncrypted">
        /// Indicates whether the message is encrypted or not.
        /// </param>
        public SecurityHeader(ISigningStrategy signingStrategy, bool isEncrypted)
        {
            _signingStrategy = signingStrategy;
            IsEncrypted = isEncrypted;
        }

        /// <summary>
        /// Gets the certificate that's being used for the signing.
        /// </summary>
        /// <value>The signing certificate.</value>
        public X509Certificate2 SigningCertificate => _signingStrategy?.SecurityTokenReference?.Certificate;

        /// <summary>
        /// Set the <see cref="ISigningStrategy"/> implementation
        /// used inside the <see cref="SecurityHeader"/>
        /// </summary>
        /// <param name="signingStrategy"></param>
        public void Sign(ISigningStrategy signingStrategy)
        {
            _signingStrategy = signingStrategy;
            _signingStrategy.SignSignature();
        }

        /// <summary>
        /// Gets the full security XML element.
        /// </summary>
        /// <returns></returns>
        public XmlElement GetXml()
        {
            var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
            XmlElement securityElement = xmlDocument
                .CreateElement("wsse", "Security", Constants.Namespaces.WssSecuritySecExt);

            _encryptionStrategy?.AppendEncryptionElements(securityElement);
            _signingStrategy?.AppendSignature(securityElement);

            return securityElement;
        }

        /// <summary>
        /// Get the Signed References from the 
        /// <see cref="ISigningStrategy"/> implementation
        /// </summary>
        /// <returns></returns>
        public ArrayList GetReferences()
        {
            return _signingStrategy == null ? new ArrayList() : _signingStrategy.GetSignedReferences();
        }

        /// <summary>
        /// Verify the Signature of the Security Header
        /// </summary>
        /// <param name="options"> The options. </param>
        /// <returns>
        /// </returns>
        public bool Verify(VerifySignatureConfig options)
        {
            return _signingStrategy.VerifySignature(options);
        }

        /// <summary>
        /// Encrypts the message and its attachments.
        /// </summary>
        /// <param name="encryptionStrategy"></param>
        public void Encrypt(IEncryptionStrategy encryptionStrategy)
        {
            _encryptionStrategy = encryptionStrategy;
            _encryptionStrategy.EncryptMessage();
            IsEncrypted = true;
        }
    }
}