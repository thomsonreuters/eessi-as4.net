using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Repositories;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Transforms;
using Eu.EDelivery.AS4.Streaming;
using CryptoReference = System.Security.Cryptography.Xml.Reference;
using Signature = Org.BouncyCastle.Asn1.Ocsp.Signature;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    /// <summary>
    /// <see cref="ISigningStrategy"/> implementation
    /// Responsible for the Signing of the <see cref="AS4Message"/>
    /// </summary>
    [Obsolete("Replaced by 2 other strategies")]
    internal class SigningStrategy : SignedXml, ISigningStrategy
    {
        private const string CidPrefix = "cid:";
        private readonly string _securityTokenReferenceNamespace;
        private readonly XmlDocument _document;

        /// <summary>
        /// Gets the security token reference used to sign the <see cref="AS4Message"/>.
        /// </summary>
        /// <value>The security token reference.</value>
        public SecurityTokenReference SecurityTokenReference { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SigningStrategy"/> class. 
        /// Create Security Header with a given Envelope Document
        /// </summary>
        /// <param name="document">
        /// </param>
        /// <param name="securityTokenReference"></param>
        internal SigningStrategy(XmlDocument document, SecurityTokenReference securityTokenReference) : base(document)
        {
            _document = document;
            SecurityTokenReference = securityTokenReference;
            _securityTokenReferenceNamespace = $"{Constants.Namespaces.WssSecuritySecExt} SecurityTokenReference";
            SignedInfo.CanonicalizationMethod = XmlDsigExcC14NTransformUrl;
        }

        /// <summary>
        /// Get the signed references from the Signature
        /// </summary>
        /// <returns></returns>
        public ArrayList GetSignedReferences()
        {
            return base.Signature.SignedInfo.References;
        }

        /// <summary>
        /// Add Certificate to the Security Header
        /// </summary>
        /// <param name="certificate"></param>
        public void AddCertificate(X509Certificate2 certificate)
        {
            SigningKey = GetSigningKeyFromCertificate(certificate);
            KeyInfo = new KeyInfo();

            KeyInfo.AddClause(SecurityTokenReference);
        }

        private static readonly object CertificateReaderLocker = new object();

        private static RSACryptoServiceProvider GetSigningKeyFromCertificate(X509Certificate2 certificate)
        {
            // When handling a large load of messages in parallel, we sometimes get a 'file is in use' exception
            // when loading the private key from the certificate.  Therefore, we synchronize access when
            // loading the private key to prevent this.
            lock (CertificateReaderLocker)
            {
                if (certificate.PrivateKey == null)
                {
                    throw new InvalidOperationException("The Private Key of the signing certificate is not present.");
                }

                var key = new RSACryptoServiceProvider();

                string keyXml = certificate.PrivateKey.ToXmlString(includePrivateParameters: true);
                key.FromXmlString(keyXml);

                return key;
            }
        }

        /// <summary>
        /// Get Element with a ID Attribute
        /// </summary>
        /// <param name="document"></param>
        /// <param name="idValue"></param>
        /// <returns></returns>
        public override XmlElement GetIdElement(XmlDocument document, string idValue)
        {
            XmlElement idElement = base.GetIdElement(document, idValue);
            return idElement ?? new SignedXmlRepository(document).GetReferenceIdElement(idValue);
        }

        /// <summary>
        /// Adds an xml reference.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hashFunction"></param>
        public void AddXmlReference(string id, string hashFunction)
        {
            var reference = new CryptoReference("#" + id) { DigestMethod = hashFunction };
            Transform transform = new XmlDsigExcC14NTransform();
            reference.AddTransform(transform);
            base.AddReference(reference);
        }

        /// <summary>
        /// Add Cic Attachment Reference
        /// </summary>
        /// <param name="attachment"></param>
        /// <param name="digestMethod"></param>
        public void AddAttachmentReference(Attachment attachment, string digestMethod)
        {
            var attachmentReference = new CryptoReference(uri: CidPrefix + attachment.Id) { DigestMethod = digestMethod };
            attachmentReference.AddTransform(new AttachmentSignatureTransform());
            base.AddReference(attachmentReference);

            SetReferenceStream(attachmentReference, attachment);
            SetAttachmentTransformContentType(attachmentReference, attachment);
            ResetReferenceStreamPosition(attachmentReference);
        }

        /// <summary>
        /// Gets the full security XML element.
        /// </summary>
        /// <param name="securityElement"></param>
        public void AppendSignature(XmlElement securityElement)
        {
            XmlNode possibleAlreadySignedDocument = _document.SelectSingleNode("//*[local-name()='Security']");

            if (possibleAlreadySignedDocument != null)
            {
                foreach (XmlNode node in possibleAlreadySignedDocument.ChildNodes)
                {
                    XmlNode importNode = securityElement.OwnerDocument.ImportNode(node, deep: true);
                    securityElement.AppendChild(importNode);
                }
            }
            else
            {
                LoadSignature();

                AddSecurityTokenReferenceToKeyInfo();
                AppendSecurityTokenElements(securityElement);
                AppendSignatureElement(securityElement);
            }
        }

        /// <summary>
        /// Loads the signature.
        /// </summary>
        public void LoadSignature()
        {
            ArrayList references = GetSignedReferences();
            if (references == null || references.Count == 0)
            {
                LoadXml(GetSignatureElement());
            }
        }

        private void AppendSignatureElement(XmlElement securityElement)
        {
            XmlElement signatureElement = base.GetXml();
            XmlNode importedSignatureElement = securityElement.OwnerDocument?.ImportNode(signatureElement, deep: true);
            if (importedSignatureElement != null)
            {
                securityElement.AppendChild(importedSignatureElement);
            }
        }

        private void AddSecurityTokenReferenceToKeyInfo()
        {
            if (!KeyInfo.OfType<SecurityTokenReference>().Any() && SecurityTokenReference != null)
            {
                KeyInfo.AddClause(SecurityTokenReference);
            }
        }

        private void AppendSecurityTokenElements(XmlElement securityElement)
        {
            foreach (SecurityTokenReference reference in KeyInfo.OfType<SecurityTokenReference>())
            {
                reference.AppendSecurityTokenTo(securityElement, securityElement.OwnerDocument);
            }
        }

        /// <summary>
        /// Returns the public key of a signature.
        /// </summary>
        protected override AsymmetricAlgorithm GetPublicKey()
        {
            AsymmetricAlgorithm publicKey = base.GetPublicKey();
            if (publicKey != null)
            {
                return publicKey;
            }

            X509Certificate2 signingCertificate = new KeyInfoRepository(KeyInfo).GetCertificate();
            if (signingCertificate != null)
            {
                publicKey = signingCertificate.PublicKey.Key;
            }

            return publicKey;
        }

        /// <summary>
        /// Add Algorithm to the Security Header
        /// </summary>
        /// <param name="algorithm"></param>
        public void AddAlgorithm(SignatureAlgorithm algorithm)
        {
            SignedInfo.SignatureMethod = algorithm.GetIdentifier();

            // Lock the SafeCanonicalizationMethods collection since, under heavy load
            // the VerifySignature method in SignedXml sometimes throws an InvalidOperationException
            // saying that the enumerate operation failed due to the fact that the collection
            // has changed.  Locking the collection seems to solve this problem.
            lock (SafeCanonicalizationMethods)
            {
                if (SafeCanonicalizationMethods.Contains(AttachmentSignatureTransform.Url) == false)
                {
                    SafeCanonicalizationMethods.Add(AttachmentSignatureTransform.Url);
                }
            }

            CryptoConfig.AddAlgorithm(algorithm.GetType(), algorithm.GetIdentifier());
            CryptoConfig.AddAlgorithm(typeof(AttachmentSignatureTransform), AttachmentSignatureTransform.Url);
            CryptoConfig.AddAlgorithm(typeof(SecurityTokenReference), _securityTokenReferenceNamespace);
        }

        /// <summary>
        /// Sign the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        public void SignSignature()
        {
            ComputeSignature();
        }

        /// <summary>
        /// Verify the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool VerifySignature(VerifySignatureConfig options)
        {
            if (!VerifyCertificate(SecurityTokenReference.Certificate, options.AllowUnknownRootCertificateAuthority, out X509ChainStatus[] status))
            {
                throw new CryptographicException($"The signing certificate is not trusted: {string.Join(" ", status.Select(s => s.StatusInformation))}");
            }

            LoadXml(GetSignatureElement());
            AddUnrecognizedAttachmentReferences(options.Attachments);

            return CheckSignature(SecurityTokenReference.Certificate, verifySignatureOnly: true);
        }

        private XmlElement GetSignatureElement()
        {
            XmlNode nodeSignature = _document.SelectSingleNode("//*[local-name()='Signature'] ");
            var xmlSignature = nodeSignature as XmlElement;
            if (nodeSignature == null || xmlSignature == null)
            {
                throw new CryptographicException("Invalid Signature: Signature Tag not found");
            }

            return xmlSignature;
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
            IEnumerable<CryptoReference> references = SignedInfo
                .References.Cast<CryptoReference>().Where(ReferenceIsCidReference()).ToArray();

            foreach (CryptoReference reference in references)
            {
                var attachment = attachments.FirstOrDefault(a => a.Matches(reference));

                if (attachment != null)
                {
                    SetReferenceStream(reference, attachment);
                    SetAttachmentTransformContentType(reference, attachment);
                }
            }
        }

        private static Func<CryptoReference, bool> ReferenceIsCidReference()
        {
            return x => x?.Uri != null && x.Uri.StartsWith(CidPrefix) && x.Uri.Length > CidPrefix.Length;
        }

        private static readonly FieldInfo RefTargetTypeField = typeof(CryptoReference).GetField("m_refTargetType", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo RefTargetField = typeof(CryptoReference).GetField("m_refTarget", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Sets the stream of a SignedInfo reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="attachment"></param>
        private static void SetReferenceStream(CryptoReference reference, Attachment attachment)
        {
            // We need reflection to set these 2 types. They are implicitly set to Xml references, 
            // but this causes problems with cid: references, since they're not part of the original stream.
            // If performance is slow on this, we can investigate the Delegate.CreateDelegate method to speed things up, 
            // however keep in mind that the reference object changes with every call, so we can't just keep the same delegate and call that.

            if (RefTargetTypeField != null)
            {
                const int streamReferenceTargetType = 0;
                RefTargetTypeField.SetValue(reference, streamReferenceTargetType);
            }

            if (RefTargetField != null)
            {
                RefTargetField.SetValue(reference, new NonCloseableStream(attachment.Content));
            }
        }

        private static void SetAttachmentTransformContentType(CryptoReference reference, Attachment attachment)
        {
            foreach (object transform in reference.TransformChain)
            {
                var attachmentTransform = transform as AttachmentSignatureTransform;

                if (attachmentTransform != null)
                {
                    attachmentTransform.ContentType = attachment.ContentType;
                }
            }
        }

        /// <summary>
        /// Resets the reference stream position to 0.
        /// </summary>
        /// <param name="reference"></param>
        private static void ResetReferenceStreamPosition(CryptoReference reference)
        {
            if (RefTargetField == null)
            {
                return;
            }

            var referenceStream = RefTargetField.GetValue(reference) as Stream;

            if (referenceStream != null)
            {
                StreamUtilities.MovePositionToStreamStart(referenceStream);
            }
        }
    }
}