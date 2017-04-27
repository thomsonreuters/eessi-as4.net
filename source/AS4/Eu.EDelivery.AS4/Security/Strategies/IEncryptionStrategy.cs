using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
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
        /// Appends all encryption elements, such as <see cref="EncryptedKey"/> and <see cref="EncryptedData"/> elements.
        /// </summary>
        /// <param name="securityElement"></param>
        void AppendEncryptionElements(XmlElement securityElement);
    }
}