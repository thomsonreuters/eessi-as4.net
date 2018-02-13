using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Transforms;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    internal class SignatureVerificationStrategy : SignatureStrategy, ISignatureVerificationStrategy
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SecurityTokenReference _securityTokenReference;
        private readonly ICertificateRepository _certificateRepository = Registry.Instance.CertificateRepository;

        internal SignatureVerificationStrategy(XmlDocument soapEnvelope) : base(soapEnvelope)
        {
            if (SafeCanonicalizationMethods.Contains(AttachmentSignatureTransform.Url) == false)
            {
                SafeCanonicalizationMethods.Add(AttachmentSignatureTransform.Url);
            }

            _securityTokenReference =
                SecurityTokenReferenceProvider.Get(soapEnvelope, SecurityTokenType.Signing, _certificateRepository);

            LoadSignature();
        }

        private static SignatureAlgorithm RetrieveSignatureAlgorithm(XmlDocument envelopeDocument)
        {
            SignatureAlgorithm algorithm = new SignatureAlgorithmProvider().Get(envelopeDocument);

            Logger.Debug($"Verify with Signature Algorithm: {algorithm.GetIdentifier()}");
            return algorithm;
        }

        /// <summary>
        /// Verify the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool VerifySignature(VerifySignatureConfig options)
        {
            if (!VerifyCertificate(_securityTokenReference.Certificate, options.AllowUnknownRootCertificateAuthority, out X509ChainStatus[] status))
            {
                throw new CryptographicException($"The signing certificate is not trusted: {string.Join(" ", status.Select(s => s.StatusInformation))}");
            }

            LoadXml(GetSignatureElement());
            AddUnrecognizedAttachmentReferences(options.Attachments);

            return CheckSignature(_securityTokenReference.Certificate, verifySignatureOnly: true);
        }

        private static bool VerifyCertificate(X509Certificate2 certificate, bool allowUnknownRootAuthority, out X509ChainStatus[] errorMessages)
        {
            using (var chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // TODO: Make this configurable

                if (allowUnknownRootAuthority)
                {
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                }

                bool isValid = chain.Build(certificate);

                errorMessages = isValid ? new X509ChainStatus[] { } : chain.ChainStatus;

                return isValid;
            }
        }

        private void AddUnrecognizedAttachmentReferences(IEnumerable<Attachment> attachments)
        {
            IEnumerable<Reference> references = SignedInfo
                .References.Cast<Reference>().Where(ReferenceIsCidReference()).ToArray();

            foreach (Reference reference in references)
            {
                var attachment = attachments.FirstOrDefault(a => a.Matches(reference));

                if (attachment != null)
                {
                    SetReferenceStream(reference, attachment);
                    SetAttachmentTransformContentType(reference, attachment);
                }
            }
        }

        private static Func<Reference, bool> ReferenceIsCidReference()
        {
            return x => x?.Uri != null && x.Uri.StartsWith(CidPrefix) && x.Uri.Length > CidPrefix.Length;
        }
    }

    public interface ISignatureVerificationStrategy
    {
        bool VerifySignature(VerifySignatureConfig options);
    }
}