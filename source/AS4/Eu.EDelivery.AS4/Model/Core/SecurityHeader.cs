using System.Collections;
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
        public bool IsEncrypted => _encryptionStrategy != null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityHeader"/> class. 
        /// Create empty <see cref="SecurityHeader"/>
        /// </summary>
        public SecurityHeader() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityHeader"/> class. 
        /// Set the used Strategies
        /// without signing the <see cref="SecurityHeader"/>
        /// This is used if the <see cref="SecurityHeader"/> 
        /// is already Signed and Encrypted with the Strategies
        /// </summary>
        /// <param name="signingStrategy">
        /// </param>
        /// <param name="encryptionStrategy">
        /// </param>
        public SecurityHeader(ISigningStrategy signingStrategy, IEncryptionStrategy encryptionStrategy)
        {
            _signingStrategy = signingStrategy;
            _encryptionStrategy = encryptionStrategy;
        }

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
        public bool Verify(VerifyConfig options)
        {
            return _signingStrategy.VerifySignature(options);
        }

        /// <summary>
        /// Decrypts the message and its attachments.
        /// </summary>
        /// <param name="encryptionStrategy"></param>
        public void Decrypt(IEncryptionStrategy encryptionStrategy)
        {
            _encryptionStrategy = encryptionStrategy;
            _encryptionStrategy.DecryptMessage();
        }

        /// <summary>
        /// Encrypts the message and its attachments.
        /// </summary>
        /// <param name="encryptionStrategy"></param>
        public void Encrypt(IEncryptionStrategy encryptionStrategy)
        {
            _encryptionStrategy = encryptionStrategy;
            _encryptionStrategy.EncryptMessage();
        }
    }
}