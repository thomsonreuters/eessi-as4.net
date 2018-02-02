using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    public interface IDecryptionStrategy
    {
        /// <summary>
        /// Decrypts the <see cref="AS4Message"/>, replacing the encrypted content with the decrypted content.
        /// </summary>
        void DecryptMessage();
    }
}