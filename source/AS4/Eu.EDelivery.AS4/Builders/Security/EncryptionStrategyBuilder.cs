using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder used to create <see cref="IEncryptionStrategy"/> implementation.
    /// </summary>
    public class EncryptionStrategyBuilder
    {
        private readonly ILogger _logger;
        private readonly EncryptionStrategy _strategy;

        private EncryptionStrategyBuilder()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionStrategyBuilder"/> class
        /// </summary>
        /// <param name="soapEnvelope"></param>
        public EncryptionStrategyBuilder(XmlDocument soapEnvelope) : this()
        {
            if (soapEnvelope == null)
                throw new ArgumentNullException(nameof(soapEnvelope));

            this._strategy = new EncryptionStrategy(soapEnvelope);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionStrategyBuilder"/> class
        /// </summary>
        /// <param name="as4Message"></param>
        public EncryptionStrategyBuilder(AS4Message as4Message) : this()
        {
            if (as4Message == null)
                throw new ArgumentNullException(nameof(as4Message));

            XmlDocument soapEnvelope = as4Message.EnvelopeDocument 
                ?? AS4XmlSerializer.Serialize(as4Message, default(CancellationToken));

            this._strategy = new EncryptionStrategy(soapEnvelope);
        }

        public EncryptionStrategyBuilder WithEncryptionAlgorithm(string encryptionAlgorithm)
        {
            this._logger.Debug($"Encryption Algorithm: {encryptionAlgorithm}");

            this._strategy.SetEncryptionAlgorithm(encryptionAlgorithm);

            return this;
        }


        public EncryptionStrategyBuilder WithCertificate(X509Certificate2 certificate)
        {
            this._logger.Debug($"Encryption certificate: {certificate.Subject}");

            this._strategy.SetCertificate(certificate);

            return this;
        }

        public EncryptionStrategyBuilder WithAttachment(Attachment attachment)
        {
            this._strategy.AddAttachment(attachment);
            return this;
        }

        public EncryptionStrategyBuilder WithAttachments(IEnumerable<Attachment> attachments)
        {
            foreach (Attachment attachment in attachments)
                WithAttachment(attachment);

            return this;
        }

        public IEncryptionStrategy Build()
        {
            return this._strategy;
        }
    }
}
