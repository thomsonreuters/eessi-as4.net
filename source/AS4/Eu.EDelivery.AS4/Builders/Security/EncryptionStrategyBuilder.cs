using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder used to create <see cref="IEncryptionStrategy"/> implementation.
    /// </summary>
    public class EncryptionStrategyBuilder
    {
        private readonly XmlDocument _soapEnvelope;
        private readonly List<Attachment> _attachments = new List<Attachment>();

        private X509Certificate2 _certificate;

        private KeyEncryptionConfiguration _keyConfiguration =
            new KeyEncryptionConfiguration(new BinarySecurityTokenReference(), KeyEncryption.Default);

        private DataEncryptionConfiguration _dataConfiguration = new DataEncryptionConfiguration(
            Encryption.Default.Algorithm,
            Encryption.Default.AlgorithmKeySize);

        private EncryptionStrategyBuilder(XmlDocument soapEnvelope)
        {
            _soapEnvelope = soapEnvelope;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionStrategyBuilder"/> class for the specified <paramref name="soapEnvelope"/>
        /// </summary>
        /// <param name="soapEnvelope"></param>
        public static EncryptionStrategyBuilder Create(XmlDocument soapEnvelope)
        {
            return new EncryptionStrategyBuilder(soapEnvelope);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionStrategyBuilder"/> class for the specified <paramref name="message"/>
        /// </summary>
        /// <param name="message"></param>
        public static EncryptionStrategyBuilder Create(MessagingContext message)
        {
            AS4Message as4Message = message.AS4Message;

            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            XmlDocument soapEnvelope = as4Message.EnvelopeDocument
               ?? AS4XmlSerializer.ToSoapEnvelopeDocument(message, default(CancellationToken));

            return new EncryptionStrategyBuilder(soapEnvelope);
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

        public EncryptionStrategyBuilder WithAttachment(Attachment attachment)
        {
            _attachments.Add(attachment);
            return this;
        }

        public EncryptionStrategyBuilder WithAttachments(IEnumerable<Attachment> attachments)
        {
            _attachments.AddRange(attachments);
            return this;
        }

        public EncryptionStrategy Build()
        {
            return new EncryptionStrategy(_soapEnvelope, _keyConfiguration, _dataConfiguration, _certificate, _attachments);
        }
    }

}
