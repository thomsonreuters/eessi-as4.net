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
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using NLog;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder used to create <see cref="ISigningStrategy" /> implementation Models
    /// </summary>
    public class SigningStrategyBuilder
    {
        private readonly ISignatureAlgorithmProvider _algorithmProvider = new SignatureAlgorithmProvider();
        private readonly List<Tuple<Attachment, string>> _attachmentReferences = new List<Tuple<Attachment, string>>();

        private readonly XmlDocument _envelopeDocument;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<Tuple<SigningId, string>> _references = new List<Tuple<SigningId, string>>();

        private readonly ISecurityTokenReferenceProvider _tokenProvider =
            new SecurityTokenReferenceProvider(Registry.Instance.CertificateRepository);

        private X509Certificate2 _certificate;
        private SecurityTokenReference _securityTokenReference;

        private SignatureAlgorithm _signatureAlgorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="SigningStrategyBuilder" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="cancellation">The cancellation.</param>
        public SigningStrategyBuilder(MessagingContext message, CancellationToken cancellation)
        {
            _envelopeDocument = AS4XmlSerializer.ToSoapEnvelopeDocument(message, cancellation);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigningStrategyBuilder" /> class.
        /// Create a new <see cref="SigningStrategyBuilder" />
        /// with given <paramref name="envelopeDocument" />
        /// </summary>
        /// <param name="envelopeDocument">
        /// </param>
        public SigningStrategyBuilder(XmlDocument envelopeDocument)
        {
            _envelopeDocument = envelopeDocument;

            _signatureAlgorithm = RetrieveSignatureAlgorithm(envelopeDocument);
            _securityTokenReference = RetrieveSigningSecurityTokenReference(envelopeDocument);
        }

        /// <summary>
        /// Build the Security Header
        /// </summary>
        /// <returns></returns>
        public ISigningStrategy Build()
        {
            var strategy = new SigningStrategy(_envelopeDocument, _securityTokenReference);

            if (_signatureAlgorithm != null)
            {
                // TODO: i believe that this is a mandatory item.
                strategy.AddAlgorithm(_signatureAlgorithm);
            }

            if (_certificate != null)
            {
                // TODO: certificate should be mandatory ?
                strategy.AddCertificate(_certificate);
            }

            foreach (Tuple<SigningId, string> reference in _references)
            {
                string hashFunction = reference.Item2;

                strategy.AddXmlReference(reference.Item1.HeaderSecurityId, hashFunction);
                strategy.AddXmlReference(reference.Item1.BodySecurityId, hashFunction);
            }

            foreach (Tuple<Attachment, string> attachmentReference in _attachmentReferences)
            {
                strategy.AddAttachmentReference(attachmentReference.Item1, attachmentReference.Item2);
            }

            return strategy;
        }

        /// <summary>
        /// Add Attachment Reference to Security Header
        /// </summary>
        /// <param name="attachment"></param>
        /// <param name="hashFunction"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithAttachment(Attachment attachment, string hashFunction)
        {
            Logger.Debug($"Signing with Attachment {attachment.Id} with Reference");

            _attachmentReferences.Add(new Tuple<Attachment, string>(attachment, hashFunction));

            return this;
        }

        /// <summary>
        /// Add a <see cref="X509Certificate2" /> to the Security Header
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithCertificate(X509Certificate2 certificate)
        {
            Logger.Debug($"Using certificate {certificate.FriendlyName}");

            if (_certificate != null)
            {
                Logger.Warn(
                    $"There is already a certificate configured ({_certificate.FriendlyName}). This one will be overwritten.");
            }

            _certificate = certificate;
            return this;
        }

        /// <summary>
        /// Add Security Token Reference to Security Header
        /// </summary>
        /// <param name="keyReferenceMethod"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSecurityTokenReference(X509ReferenceType keyReferenceMethod)
        {
            Logger.Debug($"Signing with Signature Token Reference {keyReferenceMethod}");

            _securityTokenReference = _tokenProvider.Get(keyReferenceMethod);

            return this;
        }

        /// <summary>
        /// Adds a <see cref="SignatureAlgorithm"/> to Security Header based on the given <paramref name="signatureAlgorithmIdentifier"/>.
        /// </summary>
        /// <param name="signatureAlgorithmIdentifier">Identifier to define the <see cref="SignatureAlgorithm"/>.</param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSignatureAlgorithm(string signatureAlgorithmIdentifier)
        {
            LogNewSignatureAlgorithm(signatureAlgorithmIdentifier);

            _signatureAlgorithm = _algorithmProvider.Get(signatureAlgorithmIdentifier);

            return this;
        }

        /// <summary>
        /// Adds a <see cref="SignatureAlgorithm"/> to the Security Header
        /// </summary>
        /// <param name="algorithm"><see cref="SignatureAlgorithm"/> to include inside the Security Header.</param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSignatureAlgorithm(SignatureAlgorithm algorithm)
        {
            LogNewSignatureAlgorithm(algorithm.GetType().FullName);

            _signatureAlgorithm = algorithm;

            return this;
        }

        private void LogNewSignatureAlgorithm(string algorithm)
        {
            Logger.Debug($"Setting Signing Algorithm: {algorithm}");

            if (_signatureAlgorithm != null)
            {
                Logger.Warn(
                    $"There is already a signature-algorithm configured ({_signatureAlgorithm.GetType().FullName}).  This one will be overwritten.");
            }
        }

        /// <summary>
        /// Add Signing Id to Security Header
        /// </summary>
        /// <param name="signingId"></param>
        /// <param name="hashFunction"></param>
        /// <returns></returns>
        public SigningStrategyBuilder WithSigningId(SigningId signingId, string hashFunction)
        {
            Logger.Debug($"Signing HashFunction: {hashFunction}");

            _references.Add(new Tuple<SigningId, string>(signingId, hashFunction));

            return this;
        }

        private SignatureAlgorithm RetrieveSignatureAlgorithm(XmlDocument envelopeDocument)
        {
            SignatureAlgorithm algorithm = _algorithmProvider.Get(envelopeDocument);

            Logger.Debug($"Verify with Signature Algorithm: {algorithm.GetIdentifier()}");
            return algorithm;
        }

        private SecurityTokenReference RetrieveSigningSecurityTokenReference(XmlDocument envelopeDocument)
        {
            if (envelopeDocument == null)
            {
                throw new ArgumentNullException(nameof(envelopeDocument));
            }

            XmlElement[] tokenNodes =
                envelopeDocument.SelectNodes(@"//*[local-name()='Signature']//*[local-name()='SecurityTokenReference']")
                                ?.OfType<XmlElement>().ToArray();

            if (tokenNodes != null)
            {
                Logger.Info($"{tokenNodes.Length} Signature Tokens retrieved.");

                XmlElement securityTokenElement = tokenNodes.FirstOrDefault();

                if (securityTokenElement != null)
                {
                    SecurityTokenReference token = _tokenProvider.Get(securityTokenElement, SecurityTokenType.Signing);

                    Logger.Debug($"Verify with Security Token Reference: {token.GetType().Name}");

                    return token;
                }
            }

            throw new XmlException("No Security Token Reference element found in given Xml Document");
        }
    }
}