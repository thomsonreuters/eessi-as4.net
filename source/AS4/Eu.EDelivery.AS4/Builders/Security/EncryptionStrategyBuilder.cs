using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder used to create <see cref="EncryptionStrategy"/> instance.
    /// </summary>
    internal class EncryptionStrategyBuilder
    {
        private readonly AS4Message _as4Message;
        private readonly KeyEncryptionConfiguration _keyConfiguration;

        private DataEncryptionConfiguration _dataConfiguration = new DataEncryptionConfiguration(
            Encryption.Default.Algorithm,
            Encryption.Default.AlgorithmKeySize);

        private EncryptionStrategyBuilder(AS4Message as4Message, KeyEncryptionConfiguration keyConfig)
        {
            _as4Message = as4Message;
            _keyConfiguration = keyConfig;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionStrategyBuilder"/> class for the specified <paramref name="as4Message"/>
        /// </summary>
        /// <param name="as4Message"></param>
        /// <param name="keyConfiguration">The configuration that defines how the encryption-key must be encrypted</param>
        public static EncryptionStrategyBuilder Create(AS4Message as4Message, KeyEncryptionConfiguration keyConfiguration)
        {
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            if (keyConfiguration == null)
            {
                throw new ArgumentNullException(nameof(keyConfiguration));
            }

            return new EncryptionStrategyBuilder(as4Message, keyConfiguration);
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

        public EncryptionStrategy Build()
        {
            return new EncryptionStrategy(_keyConfiguration, _dataConfiguration, _as4Message.Attachments);
        }
    }
}
