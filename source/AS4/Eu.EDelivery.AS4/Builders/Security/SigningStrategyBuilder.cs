using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
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
        private readonly ISecurityTokenReferenceProvider _tokenProvider = new SecurityTokenReferenceProvider(Registry.Instance.CertificateRepository);
        private readonly ISignatureAlgorithmProvider _algorithmProvider = new SignatureAlgorithmProvider();
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly XmlDocument _envelopeDocument;

        private SignatureAlgorithm _signatureAlgorithm = null;
        private SecurityTokenReference _securityTokenReference = null;

        private readonly List<Tuple<SigningId, string>> _references = new List<Tuple<SigningId, string>>();
        private X509Certificate2 _certificate;
        private readonly List<Tuple<Attachment, string>> _attachmentReferences = new List<Tuple<Attachment, string>>();

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
            _envelopeDocument = AS4XmlSerializer.ToDocument(as4Message, cancellationToken);
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
            _envelopeDocument = envelopeDocument;

            _signatureAlgorithm = RetrieveSignatureAlgorithm(envelopeDocument);
            _securityTokenReference = RetrieveSigningSecurityTokenReference(envelopeDocument);
        }

        private SignatureAlgorithm RetrieveSignatureAlgorithm(XmlDocument envelopeDocument)
        {
            SignatureAlgorithm algorithm = this._algorithmProvider.Get(envelopeDocument);

            this._logger.Debug($"Verify with Signature Algorithm: {algorithm.GetIdentifier()}");
            return algorithm;
        }

        private SecurityTokenReference RetrieveSigningSecurityTokenReference(XmlDocument envelopeDocument)
        {
            if (envelopeDocument == null)
            {
                throw new ArgumentNullException(nameof(envelopeDocument));
            }

            var tokenNodes =
                envelopeDocument.SelectNodes(@"//*[local-name()='Signature']//*[local-name()='SecurityTokenReference']")?.OfType<XmlElement>().ToArray();

            if (tokenNodes != null)
            {
                LogManager.GetCurrentClassLogger().Info($"{tokenNodes.Count()} Signature Tokens retrieved.");

                XmlElement securityTokenElement = tokenNodes.FirstOrDefault();

                if (securityTokenElement != null)
                {
                    SecurityTokenReference token = this._tokenProvider.Get(securityTokenElement, SecurityTokenType.Signing);

                    this._logger.Debug($"Verify with Security Token Reference: {token.GetType().Name}");

                    return token;
                }
            }

            throw AS4ExceptionBuilder.WithDescription("No Security Token Reference element found in given Xml Document")
                                       .WithErrorCode(ErrorCode.Ebms0101)
                                       .Build();

        }

        /// <summary>
        /// Add Security Token Reference to Security Header
        /// </summary>
        /// <param name="keyReferenceMethod"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSecurityTokenReference(X509ReferenceType keyReferenceMethod)
        {
            this._logger.Debug($"Signing with Signature Token Reference {keyReferenceMethod}");

            this._securityTokenReference = this._tokenProvider.Get(keyReferenceMethod);


            return this;
        }

        /// <summary>
        /// Add Signature Algorithm to Security Header
        /// </summary>
        /// <param name="signatureAlgorithmIdentifier"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSignatureAlgorithm(string signatureAlgorithmIdentifier)
        {
            this._logger.Debug($"Setting Signing Algorithm: {signatureAlgorithmIdentifier}");

            if (this._signatureAlgorithm != null)
            {
                this._logger.Warn($"There is already a signature-algorithm configured ({_signatureAlgorithm.GetType().FullName}).  This one will be overwritten.");
            }

            this._signatureAlgorithm = this._algorithmProvider.Get(signatureAlgorithmIdentifier);

            return this;
        }

        public SigningStrategyBuilder WithSignatureAlgorithm(SignatureAlgorithm algorithm)
        {
            this._logger.Debug($"Setting Signing Algorithm: {algorithm.GetType().FullName}");

            if (this._signatureAlgorithm != null)
            {
                this._logger.Warn($"There is already a signature-algorithm configured ({_signatureAlgorithm.GetType().FullName}).  This one will be overwritten.");
            }


            this._signatureAlgorithm = algorithm;

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

            this._references.Add(new Tuple<SigningId, string>(signingId, hashFunction));


            return this;
        }

        /// <summary>
        /// Add a <see cref="X509Certificate2"/> to the Security Header
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithCertificate(X509Certificate2 certificate)
        {
            this._logger.Debug($"Using certificate {certificate.FriendlyName}");

            if (this._certificate != null)
            {
                this._logger.Warn($"There is already a certificate configured ({this._certificate.FriendlyName}). This one will be overwritten.");
            }

            this._certificate = certificate;
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

            this._attachmentReferences.Add(new Tuple<Attachment, string>(attachment, hashFunction));

            return this;
        }

        /// <summary>
        /// Build the Security Header
        /// </summary>
        /// <returns></returns>
        public ISigningStrategy Build()
        {
            var strategy = new SigningStrategy(this._envelopeDocument, this._securityTokenReference);

            if (this._signatureAlgorithm != null)
            {
                // TODO: i believe that this is a mandatory item.
                strategy.AddAlgorithm(this._signatureAlgorithm);
            }

            if (this._certificate != null)
            {
                // TODO: certificate should be mandatory ?
                strategy.AddCertificate(_certificate);
            }

            foreach (var reference in this._references)
            {
                var hashFunction = reference.Item2;

                strategy.AddXmlReference(reference.Item1.HeaderSecurityId, hashFunction);
                strategy.AddXmlReference(reference.Item1.BodySecurityId, hashFunction);
            }

            foreach (var attachmentReference in this._attachmentReferences)
            {
                strategy.AddAttachmentReference(attachmentReference.Item1, attachmentReference.Item2);
            }

            return strategy;
        }
    }
}