using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.Builders;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Factories;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Serializers;
using Eu.EDelivery.AS4.Security.Transforms;
using Eu.EDelivery.AS4.Streaming;
using MimeKit;
using MimeKit.IO;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Security;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    /// <summary>
    /// An <see cref="IEncryptionStrategy"/> implementation
    /// responsible for the Encryption of the <see cref="AS4Message"/>
    /// </summary>
    public class EncryptionStrategy : EncryptedXml, IEncryptionStrategy
    {
        public const string XmlEncRSAOAEPUrlWithMgf = "http://www.w3.org/2009/xmlenc11#rsa-oaep";
        public const string XmlEncSHA1Url = "http://www.w3.org/2000/09/xmldsig#sha1";

        private readonly XmlDocument _document;
        private readonly List<Attachment> _attachments;

        private readonly KeyEncryptionConfiguration _keyEncryptionConfig;
        private readonly DataEncryptionConfiguration _dataEncryptionConfig;
        private readonly X509Certificate2 _certificate;
        private readonly List<EncryptedData> _encryptedDatas = new List<EncryptedData>();

        private AS4EncryptedKey _as4EncryptedKey;

        /// <summary>
        /// Initializes static members of the <see cref="EncryptionStrategy"/> class.
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
        /// Initializes a new instance of the <see cref="EncryptionStrategy" /> class
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="keyEncryptionConfig">The key encryption configuration.</param>
        /// <param name="dataEncryptionConfig">The data encryption configuration.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="attachments">The attachments.</param>
        /// <exception cref="ArgumentNullException">document</exception>
        internal EncryptionStrategy(
            XmlDocument document,
            KeyEncryptionConfiguration keyEncryptionConfig,
            DataEncryptionConfiguration dataEncryptionConfig,
            X509Certificate2 certificate,
            IEnumerable<Attachment> attachments) : base(document)
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
                _as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(document);

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
            {
                throw new ArgumentNullException(nameof(securityElement));
            }

            if (securityElement.OwnerDocument == null)
            {
                throw new ArgumentException(@"SecurityHeader needs to have an OwnerDocument", nameof(securityElement));
            }

            XmlDocument securityDocument = securityElement.OwnerDocument;

            // Add additional elements such as certificate references
            _keyEncryptionConfig.SecurityTokenReference.AppendSecurityTokenTo(securityElement, securityDocument);
            if (_as4EncryptedKey != null)
            {
                _as4EncryptedKey.AppendEncryptedKey(securityElement);
            }
            else
            {
                LogManager.GetCurrentClassLogger().Warn("Appending Encryption Elements but there is no AS4 Encrypted Key set.");
            }

            AppendEncryptedDataElements(securityElement, securityDocument);
        }

        private void AppendEncryptedDataElements(XmlElement securityElement, XmlDocument securityDocument)
        {
            foreach (EncryptedData encryptedData in _encryptedDatas)
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
            _encryptedDatas.Clear();

            byte[] encryptionKey = GenerateSymmetricKey(_dataEncryptionConfig.AlgorithmKeySize);
            AS4EncryptedKey as4EncryptedKey = GetEncryptedKey(encryptionKey, _certificate, _keyEncryptionConfig);

            using (SymmetricAlgorithm encryptionAlgorithm =
                CreateSymmetricAlgorithm(_dataEncryptionConfig.EncryptionMethod, encryptionKey))
            {
                EncryptAttachmentsWithAlgorithm(as4EncryptedKey, encryptionAlgorithm);
            }

            _as4EncryptedKey = as4EncryptedKey;
        }

        private static byte[] GenerateSymmetricKey(int keySize)
        {
            using (var rijn = new RijndaelManaged {KeySize = keySize})
            {
                return rijn.Key;
            }
        }

        private static AS4EncryptedKey GetEncryptedKey(
            byte[] symmetricKey,
            X509Certificate2 certificate,
            KeyEncryptionConfiguration keyEncryptionConfig)
        {
            return
                AS4EncryptedKey.CreateEncryptedKeyBuilderForKey(symmetricKey, certificate)
                               .WithEncryptionMethod(keyEncryptionConfig.EncryptionMethod)
                               .WithDigest(keyEncryptionConfig.DigestMethod)
                               .WithMgf(keyEncryptionConfig.Mgf)
                               .WithSecurityTokenReference(keyEncryptionConfig.SecurityTokenReference)
                               .Build();
        }

        private void EncryptAttachmentsWithAlgorithm(
            AS4EncryptedKey encryptedKey,
            SymmetricAlgorithm encryptionAlgorithm)
        {
            foreach (Attachment attachment in _attachments)
            {
                attachment.Content = EncryptData(attachment.Content, encryptionAlgorithm);
                EncryptedData encryptedData = CreateEncryptedDataForAttachment(attachment, encryptedKey);

                _encryptedDatas.Add(encryptedData);

                encryptedKey.AddDataReference(encryptedData.Id);
                attachment.ContentType = "application/octet-stream";
            }
        }

        private EncryptedData CreateEncryptedDataForAttachment(Attachment attachment, AS4EncryptedKey encryptedKey)
        {
            return new EncryptedDataBuilder()
                .WithDataEncryptionConfiguration(_dataEncryptionConfig)
                .WithMimeType(attachment.ContentType)
                .WithReferenceId(encryptedKey.GetReferenceId())
                .WithUri(attachment.Id)
                .Build();
        }

        private Stream EncryptData(Stream secretStream, SymmetricAlgorithm algorithm)
        {
            Stream encryptedStream = CreateVirtualStreamOf(secretStream);

            var cryptoStream = new CryptoStream(encryptedStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);
            CipherMode origMode = algorithm.Mode;
            PaddingMode origPadding = algorithm.Padding;

            try
            {
                algorithm.Mode = Mode;
                algorithm.Padding = Padding;
                secretStream.CopyTo(cryptoStream);
            }
            finally
            {
                cryptoStream.FlushFinalBlock();
                algorithm.Mode = origMode;
                algorithm.Padding = origPadding;
            }

            encryptedStream.Position = 0;

            if (Mode != CipherMode.ECB)
            {
                var chainedStream = new ChainedStream();
                chainedStream.Add(new MemoryStream(algorithm.IV));
                chainedStream.Add(encryptedStream);

                encryptedStream = chainedStream;
            }

            return encryptedStream;
        }

        /// <summary>
        /// Retrieves the decryption initialization vector (IV) from an EncryptedData  object.
        /// </summary>
        /// <returns>A byte array that contains the decryption initialization vector (IV).</returns>
        /// <param name="encryptedData"></param>
        /// <param name="symmetricAlgorithmUri"></param>        
        public override byte[] GetDecryptionIV(EncryptedData encryptedData, string symmetricAlgorithmUri)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            if (symmetricAlgorithmUri == null)
            {
                if (encryptedData.EncryptionMethod == null)
                {
                    throw new CryptographicException("Missing encryption algorithm");
                }

                symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
            }

            int vectorLength = GetIVLength(symmetricAlgorithmUri);
            var iv = new byte[vectorLength];

            Buffer.BlockCopy(encryptedData.CipherData.CipherValue, srcOffset: 0, dst: iv, dstOffset: 0, count: iv.Length);

            return iv;
        }

        private byte[] GetDecryptionIV(EncryptedData encryptedData, Stream encryptedTextStream, string symmetricAlgorithmUri)
        {
            if (encryptedData == null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            if (symmetricAlgorithmUri == null)
            {
                if (encryptedData.EncryptionMethod == null)
                {
                    throw new CryptographicException("Missing encryption algorithm");
                }

                symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
            }

            int vectorLength = GetIVLength(symmetricAlgorithmUri);
            var iv = new byte[vectorLength];

            encryptedTextStream.Read(iv, 0, iv.Length);

            return iv;
        }

        private static int GetIVLength(string symmetricAlgorithmUri)
        {
            int vectorLength;
            if (symmetricAlgorithmUri == "http://www.w3.org/2009/xmlenc11#aes128-gcm")
            {
                vectorLength = 12;
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

                    vectorLength = 16;
                }
                else
                {
                    vectorLength = 8;
                }
            }

            return vectorLength;
        }

        /// <summary>
        /// Decrypts the <see cref="AS4Message"/>, replacing the encrypted content with the decrypted content.
        /// </summary>
        public void DecryptMessage()
        {
            IEnumerable<EncryptedData> encryptedDatas =
                new EncryptedDataSerializer(_document).SerializeEncryptedDatas();

            var as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(_document);

            var sw = new Stopwatch();
            sw.Start();

            byte[] key = DecryptEncryptedKey(as4EncryptedKey);
            
            foreach (EncryptedData encryptedData in encryptedDatas)
            {
                DecryptEncryptedData(encryptedData, key);
            }
            sw.Stop();
            LogManager.GetCurrentClassLogger().Trace($"Decrypting message took {sw.ElapsedMilliseconds} milliseconds");
            _as4EncryptedKey = as4EncryptedKey;
        }

        private void DecryptEncryptedData(EncryptedData encryptedData, byte[] key)
        {
            using (SymmetricAlgorithm decryptAlgorithm =
                    CreateSymmetricAlgorithm(encryptedData.EncryptionMethod.KeyAlgorithm, key))
            {
                DecryptAttachment(encryptedData, decryptAlgorithm);
            }
        }

        private void DecryptAttachment(EncryptedData encryptedData, SymmetricAlgorithm decryptAlgorithm)
        {
            string uri = encryptedData.CipherData.CipherReference.Uri;
            Attachment attachment = _attachments.Single(x => string.Equals(x.Id, uri.Substring(4)));

            Stream decryptedStream = DecryptData(encryptedData, attachment.Content, decryptAlgorithm);

            var transformer = AttachmentTransformer.Create(encryptedData.Type);
            transformer.Transform(attachment, decryptedStream);

            attachment.ContentType = encryptedData.MimeType;
        }

        private Stream DecryptData(EncryptedData encryptedData, Stream encryptedTextStream, SymmetricAlgorithm encryptionAlgorithm)
        {
            VirtualStream decryptedStream = CreateVirtualStreamOf(encryptedTextStream);

            // save the original symmetric algorithm
            CipherMode origMode = encryptionAlgorithm.Mode;
            PaddingMode origPadding = encryptionAlgorithm.Padding;
            byte[] origIV = encryptionAlgorithm.IV;

            // read the IV from cipherValue
            byte[] decryptionIV = null;
            if (Mode != CipherMode.ECB)
            {
                decryptionIV = GetDecryptionIV(encryptedData, encryptedTextStream, null);
            }

            if (decryptionIV != null)
            {
                encryptionAlgorithm.IV = decryptionIV;
            }

            var cryptoStream = new CryptoStream(encryptedTextStream, encryptionAlgorithm.CreateDecryptor(), CryptoStreamMode.Read);

            try
            {
                encryptionAlgorithm.Mode = Mode;
                encryptionAlgorithm.Padding = Padding;
                cryptoStream.CopyTo(decryptedStream);
            }
            finally
            {
                if (!cryptoStream.HasFlushedFinalBlock)
                {
                    cryptoStream.FlushFinalBlock();
                }

                // now restore the original symmetric algorithm
                encryptionAlgorithm.Mode = origMode;
                encryptionAlgorithm.Padding = origPadding;
                encryptionAlgorithm.IV = origIV;
            }

            decryptedStream.Position = 0;

            return decryptedStream;
        }

        private static VirtualStream CreateVirtualStreamOf(Stream innerStream)
        {
            return VirtualStream.CreateVirtualStream(
                    expectedSize: innerStream.CanSeek ? innerStream.Length : VirtualStream.ThresholdMax);
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

            public abstract void Transform(Attachment attachment, Stream decryptedData);

            #region Implementations

            private class AttachmentCompleteTransformer : AttachmentTransformer
            {
                public const string Type = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Complete";

                public static readonly AttachmentCompleteTransformer Default = new AttachmentCompleteTransformer();

                public override void Transform(Attachment attachment, Stream decryptedData)
                {
                    // The decrypted data can contain MIME headers, therefore we'll need to parse
                    // the decrypted data as a MimePart, and make sure that the content is set correctly
                    // in the attachment.
                    var part = MimeEntity.Load(decryptedData) as MimePart;

                    if (part == null)
                    {
                        throw new InvalidOperationException("The decrypted stream could not be converted to a MIME part");
                    }

                    attachment.Content.Dispose();

                    attachment.Content = part.ContentObject.Stream;

                    foreach (Header header in part.Headers)
                    {
                        attachment.Properties.Add(header.Field, header.Value);
                    }
                }
            }

            private class AttachmentContentOnlyTransformer : AttachmentTransformer
            {
                public const string Type = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Only";

                public static readonly AttachmentContentOnlyTransformer Default = new AttachmentContentOnlyTransformer();

                public override void Transform(Attachment attachment, Stream decryptedData)
                {
                    attachment.Content.Dispose();
                    attachment.Content = decryptedData;
                }
            }
            #endregion
        }

        #endregion
    }
}