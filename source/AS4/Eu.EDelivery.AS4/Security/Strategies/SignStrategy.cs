using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Transforms;
using Eu.EDelivery.AS4.Serialization;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    internal class SignStrategy : SignatureStrategy
    {
        public static SignStrategy ForAS4Message(AS4Message as4Message, CalculateSignatureConfig config)
        {
            return new SignStrategy(
                as4Message,
                as4Message.EnvelopeDocument ?? AS4XmlSerializer.ToSoapEnvelopeDocument(as4Message, CancellationToken.None),
                config);
        }

        private readonly AS4Message _as4Message;
        private readonly CalculateSignatureConfig _config;

        private SignStrategy(AS4Message message, XmlDocument soapEnvelope, CalculateSignatureConfig config)
            : base(soapEnvelope)
        {
            _as4Message = message;
            _config = config;
        }

        public Signature SignDocument()
        {
            SetSigningAlgorithm(_config.SigningAlgorithm);

            SetSecurityTokenReference(_config.SigningCertificate, _config.ReferenceTokenType);

            SetSoapHeaderReferences(_as4Message.SigningId, _config.HashFunction);

            SetAttachmentReferences(_as4Message.Attachments, _config.HashFunction);

            ComputeSignature();

            ResetAttachmentContents(_as4Message);

            return Signature;
        }

        private void SetSigningAlgorithm(string signingAlgorithm)
        {
            var algorithm = SignatureAlgorithmProvider.Get(signingAlgorithm);

            SignedInfo.SignatureMethod = algorithm.GetIdentifier();

            CryptoConfig.AddAlgorithm(algorithm.GetType(), algorithm.GetIdentifier());
        }

        private void SetSecurityTokenReference(X509Certificate2 signingCertificate, X509ReferenceType securityTokenType)
        {
            var securityTokenReference = SecurityTokenReferenceProvider.Create(signingCertificate, securityTokenType);

            SigningKey = GetSigningKeyFromCertificate(signingCertificate);
            KeyInfo = new KeyInfo();

            KeyInfo.AddClause(securityTokenReference);
        }

        private void SetSoapHeaderReferences(SigningId signingId, string hashFunction)
        {
            AddXmlReference(signingId.HeaderSecurityId, hashFunction);
            AddXmlReference(signingId.BodySecurityId, hashFunction);
        }

        private void SetAttachmentReferences(IEnumerable<Attachment> attachments, string hashFunction)
        {
            foreach (var attachment in attachments)
            {
                AddAttachmentReference(attachment, hashFunction);
            }
        }

        private static readonly object CertificateReaderLocker = new object();

        private static RSA GetSigningKeyFromCertificate(X509Certificate2 certificate)
        {
            // When handling a large load of messages in parallel, we sometimes get a 'file is in use' exception
            // when loading the private key from the certificate.  Therefore, we synchronize access when
            // loading the private key to prevent this.
            lock (CertificateReaderLocker)
            {
                // Call GetRSAPrivateKey to avoid KeySet does not exist exceptions that might be thrown.
                RSA privateKey = certificate.GetRSAPrivateKey();

                if (privateKey == null)
                {
                    throw new CryptographicException("Signing certificate does not have a private key");
                }

                return privateKey;
            }
        }

        private void AddXmlReference(string id, string hashFunction)
        {
            var reference = new Reference("#" + id) { DigestMethod = hashFunction };
            Transform transform = new XmlDsigExcC14NTransform();
            reference.AddTransform(transform);
            base.AddReference(reference);
        }

        /// <summary>
        /// Add Cid Attachment Reference
        /// </summary>
        /// <param name="attachment"></param>
        /// <param name="digestMethod"></param>
        private void AddAttachmentReference(Attachment attachment, string digestMethod)
        {
            var attachmentReference = new Reference(uri: CidPrefix + attachment.Id) { DigestMethod = digestMethod };
            attachmentReference.AddTransform(new AttachmentSignatureTransform());
            base.AddReference(attachmentReference);

            SetReferenceStream(attachmentReference, attachment);
            SetAttachmentTransformContentType(attachmentReference, attachment);
            ResetReferenceStreamPosition(attachmentReference);
        }

        private static void ResetAttachmentContents(AS4Message as4Message)
        {
            foreach (Attachment attachment in as4Message.Attachments)
            {
                attachment.ResetContentPosition();
            }
        }
    }
}