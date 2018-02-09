using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Repositories;
using Eu.EDelivery.AS4.Security.Transforms;
using Eu.EDelivery.AS4.Streaming;
using Reference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    public abstract class SignatureStrategy : SignedXml
    {
        protected const string CidPrefix = "cid:";

        private readonly XmlDocument _soapEnvelope;

        static SignatureStrategy()
        {
            CryptoConfig.AddAlgorithm(typeof(AttachmentSignatureTransform), AttachmentSignatureTransform.Url);
            CryptoConfig.AddAlgorithm(typeof(SecurityTokenReference), $"{Constants.Namespaces.WssSecuritySecExt} SecurityTokenReference");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureStrategy"/> class.
        /// </summary>
        protected SignatureStrategy(XmlDocument soapEnvelope) : base(soapEnvelope)
        {
            _soapEnvelope = soapEnvelope;
            SignedInfo.CanonicalizationMethod = XmlDsigExcC14NTransformUrl;
        }

        private static readonly FieldInfo RefTargetTypeField = typeof(Reference).GetField("m_refTargetType", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo RefTargetField = typeof(Reference).GetField("m_refTarget", BindingFlags.Instance | BindingFlags.NonPublic);

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
        /// Sets the stream of a SignedInfo reference.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="attachment"></param>
        protected static void SetReferenceStream(Reference reference, Attachment attachment)
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

        protected static void SetAttachmentTransformContentType(Reference reference, Attachment attachment)
        {
            foreach (object transform in reference.TransformChain)
            {
                if (transform is AttachmentSignatureTransform attachmentTransform)
                {
                    attachmentTransform.ContentType = attachment.ContentType;
                }
            }
        }

        /// <summary>
        /// Resets the reference stream position to 0.
        /// </summary>
        /// <param name="reference"></param>
        protected static void ResetReferenceStreamPosition(Reference reference)
        {
            if (RefTargetField == null)
            {
                return;
            }

            if (RefTargetField.GetValue(reference) is Stream referenceStream)
            {
                StreamUtilities.MovePositionToStreamStart(referenceStream);
            }
        }

        /// <summary>
        /// Loads the signature.
        /// </summary>
        protected void LoadSignature()
        {
            ArrayList references = GetSignedReferences();
            if (references == null || references.Count == 0)
            {
                LoadXml(GetSignatureElement());
            }
        }

        private ArrayList GetSignedReferences()
        {
            return base.Signature.SignedInfo.References;
        }

        protected XmlElement GetSignatureElement()
        {
            XmlNode nodeSignature = _soapEnvelope.SelectSingleNode("//*[local-name()='Signature'] ");
            var xmlSignature = nodeSignature as XmlElement;
            if (nodeSignature == null || xmlSignature == null)
            {
                throw new CryptographicException("Invalid Signature: Signature Tag not found");
            }

            return xmlSignature;
        }
    }
}