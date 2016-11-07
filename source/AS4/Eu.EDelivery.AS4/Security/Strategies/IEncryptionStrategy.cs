using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    public interface IEncryptionStrategy
    {
        /// <summary>
        /// Decrypts the <see cref="AS4Message"/>, replacing the encrypted content with the decrypted content.
        /// </summary>
        void DecryptMessage();

        /// <summary>
        /// Encrypts the <see cref="AS4Message"/> and its attachments.
        /// </summary>
        void EncryptMessage();

        /// <summary>
        /// Adds an attachment to the strategy
        /// </summary>
        /// <param name="attachment"></param>
        void AddAttachment(Attachment attachment);

        /// <summary>
        /// Appends all encryption elements, such as <see cref="EncryptedKey"/> and <see cref="EncryptedData"/> elements.
        /// </summary>
        /// <param name="securityElement"></param>
        void AppendEncryptionElements(XmlElement securityElement);

        /// <summary>
        /// Sets the encryption algorithm to use.
        /// </summary>
        /// <param name="encryptionAlgorithm"></param>
        void SetEncryptionAlgorithm(string encryptionAlgorithm);

        /// <summary>
        /// Sets the certificate to use when encrypting/decrypting.
        /// </summary>
        /// <param name="certificate"></param>
        void SetCertificate(X509Certificate2 certificate);
    }
}