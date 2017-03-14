using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.Builders;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Factories;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Serializers;
using Eu.EDelivery.AS4.Security.Transforms;
using MimeKit;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    /// <summary>
    /// An <see cref="IEncryptionStrategy"/> implementation
    /// responsible for the Encryption of the <see cref="AS4Message"/>
    /// </summary>
    public class EncryptionStrategy : EncryptedXml, IEncryptionStrategy
    {
        private readonly XmlDocument _document;
        private readonly List<Attachment> _attachments;

        private readonly KeyEncryptionConfiguration _keyEncryptionConfig;
        private readonly DataEncryptionConfiguration _dataEncryptionConfig;
        private readonly X509Certificate2 _certificate;


        private readonly List<EncryptedData> _encryptedDatas = new List<EncryptedData>();

        private AS4EncryptedKey _as4EncryptedKey;

        public const string XmlEncRSAOAEPUrlWithMgf = "http://www.w3.org/2009/xmlenc11#rsa-oaep";

        public const string XmlEncSHA1Url = "http://www.w3.org/2000/09/xmldsig#sha1";

        /// <summary>
        /// Run once Crypto Configuration
        /// </summary>
        static EncryptionStrategy()
        {
            CryptoConfig.AddAlgorithm(typeof(AttachmentCiphertextTransform), AttachmentCiphertextTransform.Url);
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes128-gcm");
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes192-gcm");
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes256-gcm");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionStrategy"/> class
        /// </summary>
        /// <param name="document"></param>
        /// <param name="keyEncryptionConfig"></param>
        /// <param name="dataEncryptionConfig"></param>
        /// <param name="certificate"></param>
        /// <param name="attachments"></param>
        internal EncryptionStrategy(XmlDocument document, KeyEncryptionConfiguration keyEncryptionConfig, DataEncryptionConfiguration dataEncryptionConfig,
            X509Certificate2 certificate, IEnumerable<Attachment> attachments) : base(document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            _document = document;

            _keyEncryptionConfig = keyEncryptionConfig;
            _dataEncryptionConfig = dataEncryptionConfig;
            _attachments = attachments.ToList();

            // TODO: review this.  This is probably only necessary for decryption; maybe seperate decrypt and encrypt strategy.
            var encryptedKeyElement = document.SelectSingleNode("//*[local-name()='EncryptedKey']") as XmlElement;

            if (encryptedKeyElement != null)
            {
                this._as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(document);

                var provider = new SecurityTokenReferenceProvider(Registry.Instance.CertificateRepository);

                _keyEncryptionConfig.SecurityTokenReference = provider.Get(encryptedKeyElement, SecurityTokenType.Encryption);
            }
            // End review.

            _certificate = certificate;
            _keyEncryptionConfig.SecurityTokenReference.Certificate = certificate;
        }

        /// <summary>
        /// Appends all encryption elements, such as <see cref="EncryptedKey"/> and <see cref="EncryptedData"/> elements.
        /// </summary>
        /// <param name="securityElement"></param>
        public void AppendEncryptionElements(XmlElement securityElement)
        {
            if (securityElement == null)
                throw new ArgumentNullException(nameof(securityElement));
            if (securityElement.OwnerDocument == null)
                throw new ArgumentException(@"SecurityHeader needs to have an OwnerDocument", nameof(securityElement));

            XmlDocument securityDocument = securityElement.OwnerDocument;

            // Add additional elements such as certificate references
            this._keyEncryptionConfig.SecurityTokenReference.AppendSecurityTokenTo(securityElement, securityDocument);
            if (_as4EncryptedKey != null)
            {
                this._as4EncryptedKey.AppendEncryptedKey(securityElement);
            }
            else
            {
                NLog.LogManager.GetCurrentClassLogger().Warn("Appending Encryption Elements but there is no AS4 Encrypted Key set.");
            }

            AppendEncryptedDataElements(securityElement, securityDocument);
        }

        private void AppendEncryptedDataElements(XmlElement securityElement, XmlDocument securityDocument)
        {
            foreach (EncryptedData encryptedData in this._encryptedDatas)
            {
                XmlElement encryptedDataElement = encryptedData.GetXml();
                XmlNode importedEncryptedDataNode = securityDocument.ImportNode(encryptedDataElement, deep: true);

                securityElement.AppendChild(importedEncryptedDataNode);
            }
        }

        /// <summary>
        /// Encrypts the <see cref="AS4Message"/> and its attachments.
        /// </summary>
        public void EncryptMessage()
        {
            this._encryptedDatas.Clear();

            var encryptionKey = GenerateSymmetricKey(256);

            var as4EncryptedKey = GetEncryptedKey(encryptionKey, _keyEncryptionConfig.EncryptionMethod, _certificate, _keyEncryptionConfig.DigestMethod,
                _keyEncryptionConfig.Mgf, _keyEncryptionConfig.SecurityTokenReference);

            using (SymmetricAlgorithm encryptionAlgorithm =
                CreateSymmetricAlgorithm(_dataEncryptionConfig.EncryptionMethod, encryptionKey))
            {
                EncryptAttachmentsWithAlgorithm(as4EncryptedKey, encryptionAlgorithm);
            }

            _as4EncryptedKey = as4EncryptedKey;
        }

        private static byte[] GenerateSymmetricKey(int keySize)
        {
            using (var rijn = new RijndaelManaged { KeySize = keySize })
            {
                return rijn.Key;
            }
        }

        private static AS4EncryptedKey GetEncryptedKey(byte[] symmetricKey, string encryptionMethod, X509Certificate2 certificate, string digestAlgorithm, string mgfAlgorithm, SecurityTokenReference securityTokenReference)
        {
            var builder = AS4EncryptedKey.CreateEncryptedKeyBuilderForKey(symmetricKey, certificate)
                .WithEncryptionMethod(encryptionMethod)
                .WithDigest(digestAlgorithm)
                .WithMgf(mgfAlgorithm)
                .WithSecurityTokenReference(securityTokenReference);

            return builder.Build();
        }

        private void EncryptAttachmentsWithAlgorithm(AS4EncryptedKey encryptedKey, SymmetricAlgorithm encryptionAlgorithm)
        {
            foreach (Attachment attachment in this._attachments)
            {
                EncryptedData encryptedData = CreateEncryptedData(attachment, encryptedKey);
                EncryptAttachmentContents(attachment, encryptionAlgorithm);

                this._encryptedDatas.Add(encryptedData);
                encryptedKey.AddDataReference(encryptedData.Id);
            }
        }

        private EncryptedData CreateEncryptedData(Attachment attachment, AS4EncryptedKey encryptedKey)
        {
            return new EncryptedDataBuilder()
                .WithDataEncryptionConfiguration(this._dataEncryptionConfig)
                .WithMimeType(attachment.ContentType)
                .WithReferenceId(encryptedKey.GetReferenceId())
                .WithUri(attachment.Id)
                .Build();
        }

        private void EncryptAttachmentContents(Attachment attachment, SymmetricAlgorithm algorithm)
        {
            using (var attachmentContents = new MemoryStream())
            {
                attachment.Content.CopyTo(attachmentContents);
                byte[] encryptedBytes = base.EncryptData(attachmentContents.ToArray(), algorithm);

                attachment.Content = new MemoryStream(encryptedBytes);
                attachment.ContentType = "application/octet-stream";
            }
        }

        /// <summary>
        /// Retrieves the decryption initialization vector (IV) from an EncryptedData  object.
        /// </summary>
        /// <returns>A byte array that contains the decryption initialization vector (IV).</returns>
        /// <param name="encryptedData"></param>
        /// <param name="symmetricAlgorithmUri"></param>
        /// TODO: refactor this method!
        public override byte[] GetDecryptionIV(EncryptedData encryptedData, string symmetricAlgorithmUri)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            if (symmetricAlgorithmUri == null)
            {
                if (encryptedData.EncryptionMethod == null)
                    throw new CryptographicException("Missing encryption algorithm");
                symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
            }
            int ivLength;

            if (symmetricAlgorithmUri == "http://www.w3.org/2009/xmlenc11#aes128-gcm")
            {
                ivLength = 12;
            }
            else
            {
                if (symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#des-cbc" &&
                    symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#tripledes-cbc")
                {
                    if (symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#aes128-cbc" &&
                        symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#aes192-cbc" &&
                        symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#aes256-cbc")
                    {
                        throw new CryptographicException("Uri not supported");
                    }

                    ivLength = 16;
                }
                else
                {
                    ivLength = 8;
                }
            }
            var iv = new byte[ivLength];
            Buffer.BlockCopy(encryptedData.CipherData.CipherValue, srcOffset: 0, dst: iv, dstOffset: 0, count: iv.Length);

            return iv;
        }

        /// <summary>
        /// Decrypts the <see cref="AS4Message"/>, replacing the encrypted content with the decrypted content.
        /// </summary>
        public void DecryptMessage()
        {
            IEnumerable<EncryptedData> encryptedDatas =
                new EncryptedDataSerializer(this._document).SerializeEncryptedDatas();

            var as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(this._document);

            byte[] key = DecryptEncryptedKey(as4EncryptedKey);

            foreach (EncryptedData encryptedData in encryptedDatas)
            {
                TryDecryptEncryptedData(encryptedData, key);
            }

            _as4EncryptedKey = as4EncryptedKey;
        }

        private void TryDecryptEncryptedData(EncryptedData encryptedData, byte[] key)
        {
            try
            {
                using (SymmetricAlgorithm decryptAlgorithm =
                                CreateSymmetricAlgorithm(encryptedData.EncryptionMethod.KeyAlgorithm, key))
                {
                    DecryptAttachment(encryptedData, decryptAlgorithm);
                }
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error($"Failed to decrypt data element {ex.Message}");
                if (ex.InnerException != null)
                {
                    logger.Error(ex.InnerException.Message);
                }
                throw new AS4Exception($"Failed to decrypt data element");
            }
        }

        private void DecryptAttachment(EncryptedData encryptedData, SymmetricAlgorithm decryptAlgorithm)
        {
            string uri = encryptedData.CipherData.CipherReference.Uri;
            Attachment attachment = this._attachments.Single(x => string.Equals(x.Id, uri.Substring(4)));

            using (var attachmentInMemoryStream = new MemoryStream())
            {
                attachment.Content.CopyTo(attachmentInMemoryStream);
                encryptedData.CipherData = new CipherData(attachmentInMemoryStream.ToArray());
            }

            byte[] decryptedData = base.DecryptData(encryptedData, decryptAlgorithm);

            var transformer = AttachmentTransformer.Create(encryptedData.Type);

            transformer.Transform(attachment, decryptedData);

            attachment.ContentType = encryptedData.MimeType;
        }

        private byte[] DecryptEncryptedKey(AS4EncryptedKey encryptedKey)
        {
            OaepEncoding encoding = EncodingFactory.Instance
                .Create(encryptedKey.GetDigestAlgorithm(), encryptedKey.GetMaskGenerationFunction());

            // We do not look at the KeyInfo element in here, but rather decrypt it with the certificate provided as argument.
            AsymmetricCipherKeyPair encryptionCertificateKeyPair =
                DotNetUtilities.GetRsaKeyPair(_certificate.GetRSAPrivateKey());

            encoding.Init(false, encryptionCertificateKeyPair.Private);

            CipherData cipherData = encryptedKey.GetCipherData();
            return encoding.ProcessBlock(
                inBytes: cipherData.CipherValue, inOff: 0, inLen: cipherData.CipherValue.Length);
        }

        private static SymmetricAlgorithm CreateSymmetricAlgorithm(string name, byte[] key)
        {
            var symmetricAlgorithm = (SymmetricAlgorithm)CryptoConfig.CreateFromName(name);
            symmetricAlgorithm.Key = key;

            return symmetricAlgorithm;
        }

        #region Attachment Transformers

        private abstract class AttachmentTransformer
        {
            public static AttachmentTransformer Create(string type)
            {
                switch (type)
                {
                    case AttachmentCompleteTransformer.Type:
                        return AttachmentCompleteTransformer.Default;

                    case AttachmentContentOnlyTransformer.Type:
                        return AttachmentContentOnlyTransformer.Default;

                    default:
                        throw new NotSupportedException($"{type} is a not supported Attachment transformer.");
                }
            }

            public abstract void Transform(Attachment attachment, byte[] decryptedData);

            #region Implementations

            private class AttachmentCompleteTransformer : AttachmentTransformer
            {
                public const string Type = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Complete";

                public static readonly AttachmentCompleteTransformer Default = new AttachmentCompleteTransformer();

                public override void Transform(Attachment attachment, byte[] decryptedData)
                {
                    // The decrypted data can contain MIME headers, therefore we'll need to parse
                    // the decrypted data as a MimePart, and make sure that the content is set correctly
                    // in the attachment.
                    //var part = MimeEntity.Load(MimeKit.ContentType.Parse(decryptedData), new MemoryStream(decryptedData)) as MimePart;

                    MimePart part;

                    using (var stream = new MemoryStream(decryptedData))
                    {
                        part = MimeEntity.Load(stream) as MimePart;
                    }

                    if (part == null)
                    {
                        throw new InvalidOperationException("The decrypted stream could not be converted to a MIME part");
                    }

                    attachment.Content.Dispose();

                    attachment.Content = part.ContentObject.Stream;

                    foreach (var header in part.Headers)
                    {
                        attachment.Properties.Add(header.Field, header.Value);
                    }
                }
            }

            private class AttachmentContentOnlyTransformer : AttachmentTransformer
            {
                public const string Type = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Only";

                public static readonly AttachmentContentOnlyTransformer Default = new AttachmentContentOnlyTransformer();

                public override void Transform(Attachment attachment, byte[] decryptedData)
                {
                    attachment.Content.Dispose();
                    attachment.Content = new MemoryStream(decryptedData);
                }
            }
            #endregion
        }

        #endregion


    }


}