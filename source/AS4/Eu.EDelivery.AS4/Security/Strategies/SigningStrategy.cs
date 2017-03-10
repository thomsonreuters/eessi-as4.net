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
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Repositories;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Transforms;
using MimeKit.IO;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    /// <summary>
    /// <see cref="ISigningStrategy"/> implementation
    /// Responsible for the Signing of the <see cref="AS4Message"/>
    /// </summary>
    internal class SigningStrategy : SignedXml, ISigningStrategy
    {
        private const string CidPrefix = "cid:";
        private readonly string _securityTokenReferenceNamespace;
        private readonly XmlDocument _document;

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
            this._document = document;
            SecurityTokenReference = securityTokenReference;
            this._securityTokenReferenceNamespace = $"{Constants.Namespaces.WssSecuritySecExt} SecurityTokenReference";
            this.SignedInfo.CanonicalizationMethod = XmlDsigExcC14NTransformUrl;
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
            this.SigningKey = GetSigningKeyFromCertificate(certificate);
            this.KeyInfo = new KeyInfo();

            this.SecurityTokenReference.Certificate = certificate;
            this.KeyInfo.AddClause(this.SecurityTokenReference);
        }

        private static RSACryptoServiceProvider GetSigningKeyFromCertificate(X509Certificate2 certificate)
        {
            var cspParams = new CspParameters(24) { KeyContainerName = "XML_DISG_RSA_KEY" };
            var key = new RSACryptoServiceProvider(cspParams);
            
            string keyXml = certificate.PrivateKey.ToXmlString(includePrivateParameters: true);
            key.FromXmlString(keyXml);

            return key;
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
            LoadSignature();

            AddSecurityTokenReferenceToKeyInfo();
            AppendSecurityTokenElements(securityElement);
            AppendSignatureElement(securityElement);
        }

        private void LoadSignature()
        {
            ArrayList references = GetSignedReferences();
            if (references == null || references.Count == 0)
                this.LoadXml(GetSignatureElement());
        }

        private void AppendSignatureElement(XmlElement securityElement)
        {
            XmlElement signatureElement = base.GetXml();
            XmlNode importedSignatureElement = securityElement.OwnerDocument.ImportNode(signatureElement, deep: true);
            securityElement.AppendChild(importedSignatureElement);
        }

        private void AddSecurityTokenReferenceToKeyInfo()
        {
            if (!this.KeyInfo.OfType<SecurityTokenReference>().Any() && this.SecurityTokenReference != null)
                this.KeyInfo.AddClause(this.SecurityTokenReference);
        }

        private void AppendSecurityTokenElements(XmlElement securityElement)
        {
            foreach (SecurityTokenReference reference in this.KeyInfo.OfType<SecurityTokenReference>())
            {
                if (reference.Certificate == null)
                {
                    throw new InvalidOperationException("SecurityTokenReference does not contain certificate information");
                }
                reference.AppendSecurityTokenTo(securityElement, securityElement.OwnerDocument);
            }
        }

        /// <summary>
        /// Returns the public key of a signature.
        /// </summary>
        protected override AsymmetricAlgorithm GetPublicKey()
        {
            AsymmetricAlgorithm publicKey = base.GetPublicKey();
            if (publicKey != null) return publicKey;

            X509Certificate2 signingCertificate = new KeyInfoRepository(this.KeyInfo).GetCertificate();
            if (signingCertificate != null)
                publicKey = signingCertificate.PublicKey.Key;

            return publicKey;
        }

        /// <summary>
        /// Add Algorithm to the Security Header
        /// </summary>
        /// <param name="algorithm"></param>
        public void AddAlgorithm(SignatureAlgorithm algorithm)
        {
            this.SignedInfo.SignatureMethod = algorithm.GetIdentifier();
            this.SafeCanonicalizationMethods.Add(AttachmentSignatureTransform.Url);

            CryptoConfig.AddAlgorithm(algorithm.GetType(), algorithm.GetIdentifier());
            CryptoConfig.AddAlgorithm(typeof(AttachmentSignatureTransform), AttachmentSignatureTransform.Url);
            CryptoConfig.AddAlgorithm(typeof(SecurityTokenReference), this._securityTokenReferenceNamespace);
        }

        /// <summary>
        /// Sign the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        public void SignSignature()
        {
            base.ComputeSignature();
        }

        /// <summary>
        /// Verify the Signature of the <see cref="ISigningStrategy"/>
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool VerifySignature(VerifyConfig options)
        {
            X509ChainStatus[] status;

            if (!VerifyCertificate(this.SecurityTokenReference.Certificate, out status))
            {

                throw ThrowAS4SignException($"The signing certificate is not trusted: {string.Join(" ", status.Select(s => s.StatusInformation))}");
            }

            this.LoadXml(GetSignatureElement());
            this.AddUnreconizedAttachmentReferences(options.Attachments);

            return this.CheckSignature(this.SecurityTokenReference.Certificate, verifySignatureOnly: true);
        }

        private XmlElement GetSignatureElement()
        {
            XmlNode nodeSignature = this._document.SelectSingleNode("//*[local-name()='Signature'] ");
            var xmlSignature = nodeSignature as XmlElement;
            if (nodeSignature == null || xmlSignature == null)
                throw ThrowAS4SignException("Invalid Signature: Signature Tag not found");

            return xmlSignature;
        }
        
        private static bool VerifyCertificate(X509Certificate2 certificate, out X509ChainStatus[] errorMessages)
        {
            using (X509Chain chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // TODO: Make this configurable
                bool isValid = chain.Build(certificate);

                errorMessages = isValid ? new X509ChainStatus[] { } : chain.ChainStatus;

                return isValid;
            }
        }

        private static AS4Exception ThrowAS4SignException(string description)
        {
            return AS4ExceptionBuilder
                .WithDescription(description)
                .WithErrorCode(ErrorCode.Ebms0101)
                .Build();
        }

        private void AddUnreconizedAttachmentReferences(ICollection<Attachment> attachments)
        {
            IEnumerable<CryptoReference> references = this.SignedInfo
                .References.Cast<CryptoReference>().Where(ReferenceIsCidReference());

            foreach (CryptoReference reference in references)
            {
                string pureReferenceId = reference.Uri.Substring(CidPrefix.Length);
                Attachment attachment = attachments.FirstOrDefault(x => x.Id.Equals(pureReferenceId));
                SetReferenceStream(reference, attachment);
                SetAttachmentTransformContentType(reference, attachment);
            }
        }

        private static Func<CryptoReference, bool> ReferenceIsCidReference()
        {
            return x => x?.Uri != null && x.Uri.StartsWith(CidPrefix) && x.Uri.Length > CidPrefix.Length;
        }

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
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo fieldInfo = typeof(CryptoReference).GetField("m_refTargetType", bindingFlags);

            const int streamReferenceTargetType = 0;
            fieldInfo?.SetValue(reference, streamReferenceTargetType);

            fieldInfo = typeof(CryptoReference).GetField("m_refTarget", bindingFlags);
            fieldInfo?.SetValue(reference, new NonCloseableStream(attachment.Content));
        }

        private static void SetAttachmentTransformContentType(CryptoReference reference, Attachment attachment)
        {
            foreach (object transform in reference.TransformChain)
            {
                var attachmentTransform = transform as AttachmentSignatureTransform;
                if (attachmentTransform != null)
                    attachmentTransform.ContentType = attachment.ContentType;
            }
        }

        /// <summary>
        /// Resets the reference stream position to 0.
        /// </summary>
        /// <param name="reference"></param>
        private static void ResetReferenceStreamPosition(CryptoReference reference)
        {
            FieldInfo fieldInfo = typeof(CryptoReference).GetField(
                "m_refTarget",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null) return;
            var referenceStream = fieldInfo.GetValue(reference) as Stream;

            Stream streamToWorkOn = referenceStream;
            if (streamToWorkOn != null)
            {
                streamToWorkOn.Position = 0;
                if (referenceStream is NonCloseableStream)
                    streamToWorkOn = (referenceStream as NonCloseableStream).InnerStream;
            }
            if (referenceStream is FilteredStream)
                (referenceStream as FilteredStream).Source.Position = 0;
        }
    }
}