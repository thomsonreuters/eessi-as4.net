using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder used to create <see cref="ISigningStrategy"/> implementation Models
    /// </summary>
    public class SigningStrategyBuilder
    {
        private readonly SigningStrategy _strategy;

        private ISecurityTokenReferenceProvider _tokenProvider;
        private ISignatureAlgorithmProvider _algorithmProvider;
        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SigningStrategyBuilder"/> class. 
        /// Create new <see cref="SigningStrategyBuilder"/> 
        /// with given <paramref name="as4Message"/>
        /// </summary>
        /// <param name="as4Message">
        /// </param>
        /// <param name="cancellationToken">
        /// </param>
        public SigningStrategyBuilder(AS4Message as4Message, CancellationToken cancellationToken)
        {
            InitializeFields();

            XmlDocument envelopeDocument = AS4XmlSerializer.Serialize(as4Message, cancellationToken);
            this._strategy = new SigningStrategy(envelopeDocument);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigningStrategyBuilder"/> class. 
        /// Create a new <see cref="SigningStrategyBuilder"/> 
        /// with given <paramref name="envelopeDocument"/>
        /// </summary>
        /// <param name="envelopeDocument">
        /// </param>
        public SigningStrategyBuilder(XmlDocument envelopeDocument)

        {
            InitializeFields();

            this._strategy = new SigningStrategy(envelopeDocument);
            LoadSignatureAlgorithm(envelopeDocument);
            LoadSecurityTokenReference(envelopeDocument);

            this._logger = LogManager.GetCurrentClassLogger();
        }

        private void InitializeFields()
        {
            this._tokenProvider = new SecurityTokenReferenceProvider();
            this._algorithmProvider = new SignatureAlgorithmProvider();
            this._logger = LogManager.GetCurrentClassLogger();
        }

        private void LoadSignatureAlgorithm(XmlDocument envelopeDocument)
        {
            SignatureAlgorithm algorithm = this._algorithmProvider.Get(envelopeDocument);
            this._strategy.AddAlgorithm(algorithm);
            this._logger.Debug($"Verify with Signature Algorithm: {algorithm.GetIdentifier()}");
        }

        private void LoadSecurityTokenReference(XmlDocument envelopeDocument)
        {
            SecurityTokenReference securityTokenReference = this._tokenProvider.Get(envelopeDocument);
            this._strategy.SecurityTokenReference = securityTokenReference;
            this._logger.Debug($"Verify with Security Token Reference: {securityTokenReference.GetType().Name}");
        }

        /// <summary>
        /// Add Security Token Reference to Security Header
        /// </summary>
        /// <param name="keyReferenceMethod"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSecurityTokenReference(X509ReferenceType keyReferenceMethod)
        {
            this._logger.Debug($"Signing with Signature Token Reference {keyReferenceMethod}");
            this._strategy.SecurityTokenReference = this._tokenProvider.Get(keyReferenceMethod);

            return this;
        }

        /// <summary>
        /// Add Signature Algorithm to Security Header
        /// </summary>
        /// <param name="signatureAlgorithmIdentifier"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSignatureAlgorithm(string signatureAlgorithmIdentifier)
        {
            this._logger.Debug($"Signing Algorithm: {signatureAlgorithmIdentifier}");
            SignatureAlgorithm signatureAlgorithm = this._algorithmProvider.Get(signatureAlgorithmIdentifier);
            this._strategy.AddAlgorithm(signatureAlgorithm);

            return this;
        }

        /// <summary>
        /// Add Signing Id to Security Header
        /// </summary>
        /// <param name="signingId"></param>
        /// <param name="hashFunction"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSigningId(SigningId signingId, string hashFunction)
        {
            this._logger.Debug($"Signing HashFunction: {hashFunction}");
            this._strategy.AddXmlReference(signingId.HeaderSecurityId, hashFunction);
            this._strategy.AddXmlReference(signingId.BodySecurityId, hashFunction);

            return this;
        }

        /// <summary>
        /// Add a <see cref="X509Certificate2"/> to the Security Header
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithCertificate(X509Certificate2 certificate)
        {
            this._strategy.AddCertificate(certificate);

            return this;
        }

        /// <summary>
        /// Add Attachment Reference to Security Header
        /// </summary>
        /// <param name="attachment"></param>
        /// <param name="hashFunction"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithAttachment(Attachment attachment, string hashFunction)
        {
            this._logger.Debug($"Signing with Attachment {attachment.Id} with Reference");
            this._strategy.AddAttachmentReference(attachment, hashFunction);

            return this;
        }

        /// <summary>
        /// Build the Security Header
        /// </summary>
        /// <returns></returns>
        public ISigningStrategy Build()
        {
            return this._strategy;
        }
    }
}