using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder used to create an <see cref="DecryptionStrategy"/> instance.
    /// </summary>
    internal class DecryptionStrategyBuilder
    {
        private readonly AS4Message _message;

        private X509Certificate2 _certificate;

        private DecryptionStrategyBuilder(AS4Message message)
        {
            _message = message;
        }

        /// <summary>
        /// Create a builder instance for the given <paramref name="as4Message"/>
        /// </summary>
        /// <param name="as4Message"></param>
        /// <returns></returns>
        public static DecryptionStrategyBuilder Create(AS4Message as4Message)
        {
            return new DecryptionStrategyBuilder(as4Message);
        }

        /// <summary>
        /// Specify the certificate that must be used to decrypt.
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public DecryptionStrategyBuilder WithCertificate(X509Certificate2 certificate)
        {
            _certificate = certificate;
            return this;
        }

        /// <summary>
        /// Build the IDecryptionStrategy implementation.
        /// </summary>
        /// <returns></returns>
        public DecryptionStrategy Build()
        {
            return new DecryptionStrategy(_message.EnvelopeDocument, _message.Attachments, _certificate);
        }
    }
}