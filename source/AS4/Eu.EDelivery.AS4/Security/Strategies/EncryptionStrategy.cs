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

        private readonly EncryptionConfiguration _configuration;        
        private readonly List<EncryptedData> _encryptedDatas;
        private readonly AS4EncryptedKey _as4EncryptedKey;

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
        internal EncryptionStrategy(XmlDocument document) : base(document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            this._document = document;
            this._configuration = new EncryptionConfiguration();
            this._attachments = new List<Attachment>();
            this._encryptedDatas = new List<EncryptedData>();
            this._as4EncryptedKey = new AS4EncryptedKey();

            var encryptedKeyElement = document.SelectSingleNode("//*[local-name()='EncryptedKey']") as XmlElement;
            if (encryptedKeyElement != null)
            {
                var provider = new SecurityTokenReferenceProvider(Registry.Instance.CertificateRepository);

                this._configuration.Key.SecurityTokenReference = provider.Get(encryptedKeyElement);                    
            }
        }
        
        /// <summary>
        /// Adds an <see cref="Attachment"/>, which the strategy can use later on in the encryption/decryption logic.
        /// </summary>
        /// <param name="attachment"></param>
        public void AddAttachment(Attachment attachment)
        {
            this._attachments.Add(attachment);
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
            this._configuration.Key.SecurityTokenReference.AppendSecurityTokenTo(securityElement, securityDocument);
            this._as4EncryptedKey.AppendEncryptedKey(securityElement);
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
        /// Sets the encryption algorithm to use.
        /// </summary>
        /// <param name="encryptionAlgorithm"></param>
        public void SetEncryptionAlgorithm(string encryptionAlgorithm)
        {
            this._configuration.Data.EncryptionMethod = encryptionAlgorithm;
        }

        /// <summary>
        /// Sets the certificate to use when encrypting/decrypting.
        /// </summary>
        /// <param name="certificate"></param>
        public void SetCertificate(X509Certificate2 certificate)
        {
            this._configuration.Certificate = certificate;
        }

        /// <summary>
        /// Encrypts the <see cref="AS4Message"/> and its attachments.
        /// </summary>
        public void EncryptMessage()
        {
            this._encryptedDatas.Clear();

            OaepEncoding encoding = CreateOaepEncoding();
            byte[] encryptionKey = GenerateSymmetricKey(encoding.GetOutputBlockSize());
            SetEncryptedKey(encoding, encryptionKey);

            SymmetricAlgorithm encryptionAlgorithm = CreateSymmetricAlgorithm(this._configuration.Data.EncryptionMethod, encryptionKey);
            EncryptAttachmentsWithAlgorithm(encryptionAlgorithm);
        }

        private OaepEncoding CreateOaepEncoding()
        {
            OaepEncoding encoding = EncodingFactory.Instance
                .Create(this._configuration.Key.DigestMethod);

            RSA rsaPublicKey = this._configuration.Certificate.GetRSAPublicKey();
            RsaKeyParameters publicKey = DotNetUtilities.GetRsaPublicKey(rsaPublicKey);
            encoding.Init(forEncryption: true, param: publicKey);

            return encoding;
        }

        private byte[] GenerateSymmetricKey(int keySize)
        {
            using (var rijn = new RijndaelManaged {KeySize = keySize}) return rijn.Key;
        }

        private void SetEncryptedKey(OaepEncoding encoding, byte[] symmetricKey)
        {
            var builder = new EncryptedKeyBuilder()
                .WithEncoding(encoding)
                .WithSymmetricKey(symmetricKey)
                .WithSecurityTokenReference(this._configuration.Key.SecurityTokenReference);

            this._as4EncryptedKey.SetEncryptedKey(builder.Build());
        }

        private void EncryptAttachmentsWithAlgorithm(SymmetricAlgorithm encryptionAlgorithm)
        {
            foreach (Attachment attachment in this._attachments)
            {
                EncryptedData encryptedData = CreateEncryptedData(attachment);
                EncryptAttachmentContents(attachment, encryptionAlgorithm);

                this._encryptedDatas.Add(encryptedData);
                this._as4EncryptedKey.AddDataReference(encryptedData.Id);
            }
        }

        private EncryptedData CreateEncryptedData(Attachment attachment)
        {
            return new EncryptedDataBuilder()
                .WithDataEncryptionConfiguration(this._configuration.Data)
                .WithMimeType(attachment.ContentType)
                .WithReferenceId(this._as4EncryptedKey.GetReferenceId())
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
                throw new ArgumentNullException(nameof(encryptedData));
            if (symmetricAlgorithmUri == null)
            {
                if (encryptedData.EncryptionMethod == null)
                    throw new CryptographicException("Missing encryption algorithm");
                symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
            }
            int ivLength;

            if (symmetricAlgorithmUri == "http://www.w3.org/2009/xmlenc11#aes128-gcm") ivLength = 12;
            else
            {
                if (symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#des-cbc" &&
                    symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#tripledes-cbc")
                {
                    if (symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#aes128-cbc" &&
                        symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#aes192-cbc" &&
                        symmetricAlgorithmUri != "http://www.w3.org/2001/04/xmlenc#aes256-cbc")
                        throw new CryptographicException("Uri not supported");
                    ivLength = 16;
                }
                else ivLength = 8;
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

            foreach (EncryptedData encryptedData in encryptedDatas)
                TryDecryptEncryptedData(encryptedData);
        }

        private void TryDecryptEncryptedData(EncryptedData encryptedData)
        {
            try
            {
                this._as4EncryptedKey.LoadEncryptedKey(this._document);

                byte[] key = DecryptEncryptedKey();
                SymmetricAlgorithm decryptAlgorithm = CreateSymmetricAlgorithm(
                    encryptedData.EncryptionMethod.KeyAlgorithm, key);

                DecryptAttachment(encryptedData, decryptAlgorithm);
            }
            catch (Exception)
            {
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
            attachment.Content.Dispose();
            attachment.Content = new MemoryStream(decryptedData);
            attachment.ContentType = encryptedData.MimeType;
        }

        private byte[] DecryptEncryptedKey()
        {
            OaepEncoding encoding = EncodingFactory.Instance
                .Create(this._as4EncryptedKey.GetDigestAlgorithm());

            // We do not look at the KeyInfo element in here, but rather decrypt it with the certificate provided as argument.
            AsymmetricCipherKeyPair encryptionCertificateKeyPair =
                DotNetUtilities.GetRsaKeyPair(this._configuration.Certificate.GetRSAPrivateKey());

            encoding.Init(false, encryptionCertificateKeyPair.Private);

            CipherData cipherData = this._as4EncryptedKey.GetCipherData();
            return encoding.ProcessBlock(
                inBytes: cipherData.CipherValue, inOff: 0, inLen: cipherData.CipherValue.Length);
        }

        private SymmetricAlgorithm CreateSymmetricAlgorithm(string name, byte[] key)
        {
            var symmetricAlgorithm = (SymmetricAlgorithm)CryptoConfig.CreateFromName(name);
            symmetricAlgorithm.Key = key;

            return symmetricAlgorithm;
        }
    }
}