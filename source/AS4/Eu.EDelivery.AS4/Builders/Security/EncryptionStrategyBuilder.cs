using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder used to create <see cref="IEncryptionStrategy"/> implementation.
    /// </summary>
    public class EncryptionStrategyBuilder
    {
        private readonly AS4Message _as4Message;

        private X509Certificate2 _certificate;

        private KeyEncryptionConfiguration _keyConfiguration =
            new KeyEncryptionConfiguration(new BinarySecurityTokenReference(), KeyEncryption.Default);

        private DataEncryptionConfiguration _dataConfiguration = new DataEncryptionConfiguration(
            Encryption.Default.Algorithm,
            Encryption.Default.AlgorithmKeySize);

        private EncryptionStrategyBuilder(AS4Message as4Message)
        {
            _as4Message = as4Message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionStrategyBuilder"/> class for the specified <paramref name="as4Message"/>
        /// </summary>
        /// <param name="as4Message"></param>
        public static EncryptionStrategyBuilder Create(AS4Message as4Message)
        {
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            return new EncryptionStrategyBuilder(as4Message);
        }

        /// <summary>
        /// With the key encryption configuration.
        /// </summary>
        /// <param name="keyEncryptionConfig">The key encryption configuration.</param>
        /// <returns></returns>
        public EncryptionStrategyBuilder WithKeyEncryptionConfiguration(KeyEncryptionConfiguration keyEncryptionConfig)
        {
            _keyConfiguration = keyEncryptionConfig;
            return this;
        }

        /// <summary>
        /// With the data encryption configuration.
        /// </summary>
        /// <param name="dataEncryptionConfig">The data encryption configuration.</param>
        /// <returns></returns>
        public EncryptionStrategyBuilder WithDataEncryptionConfiguration(
            DataEncryptionConfiguration dataEncryptionConfig)
        {
            _dataConfiguration = dataEncryptionConfig;
            return this;
        }

        /// <summary>
        /// Withes the certificate.
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <returns></returns>
        public EncryptionStrategyBuilder WithCertificate(X509Certificate2 certificate)
        {
            _certificate = certificate;
            return this;
        }

        public EncryptionStrategy Build()
        {
            return new EncryptionStrategy(_keyConfiguration, _dataConfiguration, _certificate, _as4Message.Attachments);
        }
    }
}
