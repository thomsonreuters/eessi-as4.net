using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Transforms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    public class EncryptionStrategy : EncryptedXml, IEncryptionStrategy
    {
        private readonly XmlDocument _document;
        private readonly List<Attachment> _attachments = new List<Attachment>();

        internal EncryptionConfiguration Configuration { get; } = new EncryptionConfiguration();

        public List<EncryptedData> EncryptedDatas { get; } = new List<EncryptedData>();
        public EncryptedKey EncryptedKey { get; set; }

        static EncryptionStrategy()
        {
            CryptoConfig.AddAlgorithm(typeof(AttachmentCiphertextTransform), AttachmentCiphertextTransform.Url);

            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes128-gcm");
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes192-gcm");
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes256-gcm");
        }

        internal EncryptionStrategy(XmlDocument document) : base(document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            this._document = document;
        }

        /// <summary>
        /// Decrypts the <see cref="AS4Message"/>, replacing the encrypted content with the decrypted content.
        /// </summary>
        public void DecryptMessage()
        {
            IEnumerable<XmlElement> encryptedDataElements = SelectEncryptedDataElements();
            DecryptEncryptedDataElements(encryptedDataElements);
        }

        private IEnumerable<XmlElement> SelectEncryptedDataElements()
        {
            var namespaceManager = new XmlNamespaceManager(this._document.NameTable);
            namespaceManager.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");

            IEnumerable<XmlElement> encryptedDataElements = this._document
                .SelectNodes("//enc:EncryptedData", namespaceManager)?.OfType<XmlElement>();

            if (encryptedDataElements == null)
                throw new AS4Exception("No EncryptedData elements found to decrypt");

            return encryptedDataElements;
        }

        private void DecryptEncryptedDataElements(IEnumerable<XmlElement> encryptedDataElements)
        {
            foreach (XmlElement encryptedDataElement in encryptedDataElements)
                TryDecryptEncryptedDataElement(encryptedDataElement, this.Configuration.Certificate, this._attachments);
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
            this.Configuration.Key.SecurityTokenReference.AddSecurityTokenTo(securityElement, securityDocument);

            AppendEncryptedKeyElement(securityElement, securityDocument);
            AppendEncryptedDataElements(securityElement, securityDocument);
        }

        private void AppendEncryptedKeyElement(XmlElement securityElement, XmlDocument securityDocument)
        {
            XmlElement encryptedKeyElement = GetEncryptedKeyElement();
            securityElement.AppendChild(securityDocument.ImportNode(encryptedKeyElement, deep: true));
        }

        private void AppendEncryptedDataElements(XmlElement securityElement, XmlDocument securityDocument)
        {
            foreach (EncryptedData encryptedData in this.EncryptedDatas)
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
            this.Configuration.Data.EncryptionMethod = encryptionAlgorithm;
        }

        /// <summary>
        /// Sets the certificate to use when encrypting/decrypting.
        /// </summary>
        /// <param name="certificate"></param>
        public void SetCertificate(X509Certificate2 certificate)
        {
            this.Configuration.Certificate = certificate;
        }

        /// <summary>
        /// Encrypts the <see cref="AS4Message"/> and its attachments.
        /// </summary>
        public void EncryptMessage()
        {
            this.EncryptedDatas.Clear();

            OaepEncoding encoding = CreateOaepEncoding();
            byte[] encryptionKey = GenerateSymmetricKey(encoding.GetOutputBlockSize());
            this.EncryptedKey = EncryptKey(encryptionKey, encoding);

            SymmetricAlgorithm encryptionAlgorithm = CreateSymmetricEncryptionAlgorithm(encryptionKey);
            EncryptAttachmentsWithAlgorithm(encryptionAlgorithm);
        }

        private void EncryptAttachmentsWithAlgorithm(SymmetricAlgorithm encryptionAlgorithm)
        {
            foreach (Attachment attachment in this._attachments)
            {
                EncryptedData encryptedData = EncryptAttachment(attachment, encryptionAlgorithm);

                this.EncryptedDatas.Add(encryptedData);
                // Add a reference in the key to this encryptedData element
                this.EncryptedKey.ReferenceList.Add(new DataReference(encryptedData.Id));

                encryptedData.MimeType = attachment.ContentType;
                attachment.ContentType = "application/octet-stream";
            }
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
                throw new ArgumentNullException(nameof(encryptedData));
            if (symmetricAlgorithmUri == null)
            {
                if (encryptedData.EncryptionMethod == null)
                    throw new CryptographicException("Missing encryption algorithm");
                symmetricAlgorithmUri = encryptedData.EncryptionMethod.KeyAlgorithm;
            }
            int ivLength;

            if (symmetricAlgorithmUri == "http://www.w3.org/2009/xmlenc11#aes128-gcm")
                ivLength = 12;

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
                else
                    ivLength = 8;
            }
            var iv = new byte[ivLength];
            Buffer.BlockCopy(encryptedData.CipherData.CipherValue, srcOffset: 0, dst: iv, dstOffset: 0, count: iv.Length);

            return iv;
        }

        private OaepEncoding CreateOaepEncoding()
        {
            RSA rsaPublicKey = this.Configuration.Certificate.GetRSAPublicKey();
            RsaKeyParameters publicKey = DotNetUtilities.GetRsaPublicKey(rsaPublicKey);

            IDigest digestFromUri = CreateDigestFromUri(this.Configuration.Key.DigestMethod);
            var encoding = new OaepEncoding(
                cipher: new RsaEngine(), hash: digestFromUri, mgf1Hash: new Sha1Digest(), encodingParams: new byte[0]);

            encoding.Init(forEncryption: true, param: publicKey);

            return encoding;
        }

        private SymmetricAlgorithm CreateSymmetricEncryptionAlgorithm(byte[] symmetricKey)
        {
            var symmetricEncryptionAlgorithm =
                (SymmetricAlgorithm) CryptoConfig.CreateFromName(this.Configuration.Data.EncryptionMethod);
            symmetricEncryptionAlgorithm.Key = symmetricKey;

            return symmetricEncryptionAlgorithm;
        }

        private byte[] GenerateSymmetricKey(int keySize)
        {
            using (var rijn = new RijndaelManaged {KeySize = keySize}) return rijn.Key;
        }

        private EncryptedKey EncryptKey(byte[] symmetricKey, OaepEncoding encoding)
        {
            EncryptedKey encryptedKey = CreateEncryptedKey();
            encryptedKey.CipherData.CipherValue = encoding.ProcessBlock(
                symmetricKey, inOff: 0, inLen: symmetricKey.Length);
            encryptedKey.KeyInfo.AddClause(this.Configuration.Key.SecurityTokenReference);

            return encryptedKey;
        }

        private EncryptedKey CreateEncryptedKey()
        {
            return new EncryptedKey
            {
                Id = "ek-" + Guid.NewGuid(),
                EncryptionMethod = new EncryptionMethod(XmlEncRSAOAEPUrl),
                CipherData = new CipherData()
            };
        }

        private XmlElement GetEncryptedKeyElement()
        {
            XmlElement encryptedKeyElement = this.EncryptedKey.GetXml();
            AppendDigestMethod(encryptedKeyElement.SelectSingleNode("//*[local-name()='EncryptionMethod']"));

            return encryptedKeyElement;
        }

        private void AppendDigestMethod(XmlNode encryptionMethodNode)
        {
            XmlElement digestMethod = encryptionMethodNode.OwnerDocument
                .CreateElement("DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
            digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");

            encryptionMethodNode.AppendChild(digestMethod);
        }

        private EncryptedData EncryptAttachment(Attachment attachment, SymmetricAlgorithm algorithm)
        {
            EncryptAttachmentContents(attachment, algorithm);
            EncryptedData encryptedData = CreateAttachmentEncryptedDataElement(attachment);

            return encryptedData;
        }

        private void EncryptAttachmentContents(Attachment attachment, SymmetricAlgorithm algorithm)
        {
            using (var attachmentContents = new MemoryStream())
            {
                attachment.Content.CopyTo(attachmentContents);
                byte[] encryptedBytes = EncryptData(attachmentContents.ToArray(), algorithm);
                attachment.Content = new MemoryStream(encryptedBytes);
            }
        }

        private EncryptedData CreateAttachmentEncryptedDataElement(Attachment attachment)
        {
            EncryptedData encryptedData = CreateEncryptedData();
            encryptedData.CipherData.CipherReference = new CipherReference("cid:" + attachment.Id);
            encryptedData.CipherData.CipherReference.TransformChain.Add(new AttachmentCiphertextTransform());
            encryptedData.KeyInfo.AddClause(new ReferenceSecurityTokenReference {ReferenceId = this.EncryptedKey.Id});

            return encryptedData;
        }

        private EncryptedData CreateEncryptedData()
        {
            return new EncryptedData
            {
                Id = "ed-" + Guid.NewGuid(),
                Type = this.Configuration.Data.EncryptionType,
                EncryptionMethod = new EncryptionMethod(this.Configuration.Data.EncryptionMethod),
                CipherData = new CipherData()
            };
        }

        private IDigest CreateDigestFromUri(string uri)
        {
            return DigestUtilities.GetDigest(uri.Substring(uri.IndexOf('#') + 1));
        }

        private void TryDecryptEncryptedDataElement(
            XmlElement encryptedDataElement, X509Certificate2 decryptCertificate, ICollection<Attachment> attachments)
        {
            try
            {
                EncryptedData encryptedData = LoadEncryptedData(encryptedDataElement);
                LoadEncryptedKey();
                SymmetricAlgorithm decryptAlgorithm = this.GetDecryptionAlgorithm(encryptedData, decryptCertificate);
                string uri = encryptedData.CipherData.CipherReference.Uri;
                Attachment attachment = attachments.Single(x => string.Equals(x.Id, uri.Substring(4)));

                using (var attachmentInMemoryStream = new MemoryStream())
                {
                    attachment.Content.CopyTo(attachmentInMemoryStream);
                    encryptedData.CipherData = new CipherData(attachmentInMemoryStream.ToArray());
                }

                byte[] decryptedData = this.DecryptData(encryptedData, decryptAlgorithm);
                attachment.Content.Dispose();
                attachment.Content = new MemoryStream(decryptedData);
                attachment.ContentType = encryptedData.MimeType;
            }
            catch (Exception)
            {
                throw new AS4Exception($"Failed to decrypt data element");
            }
        }

        private EncryptedData LoadEncryptedData(XmlElement encryptedDataElement)
        {
            var encryptedData = new EncryptedData();
            encryptedData.LoadXml(encryptedDataElement);

            return encryptedData;
        }

        private void LoadEncryptedKey()
        {
            this.EncryptedKey = new EncryptedKey();
            this.EncryptedKey.LoadXml(this._document.SelectSingleNode("//*[local-name()='EncryptedKey']") as XmlElement);
        }

        private SymmetricAlgorithm GetDecryptionAlgorithm(
            EncryptedData encryptedData, X509Certificate2 decryptCertificate)
        {
            byte[] key = this.DecryptEncryptedKey(this.EncryptedKey, decryptCertificate);

            var decryptionAlgorithm =
                (SymmetricAlgorithm) CryptoConfig.CreateFromName(encryptedData.EncryptionMethod.KeyAlgorithm);
            decryptionAlgorithm.Key = key;

            return decryptionAlgorithm;
        }

        private byte[] DecryptEncryptedKey(EncryptedKey encryptedKey, X509Certificate2 decryptCertificate)
        {
            OaepEncoding encoding = CreateDecryptEncoding(encryptedKey);

            // We do not look at the KeyInfo element in here, but rather decrypt it with the certificate provided as argument.
            AsymmetricCipherKeyPair encryptionCertificateKeyPair =
                DotNetUtilities.GetRsaKeyPair(decryptCertificate.GetRSAPrivateKey());

            encoding.Init(false, encryptionCertificateKeyPair.Private);

            return encoding.ProcessBlock(
                inBytes: encryptedKey.CipherData.CipherValue,
                inOff: 0,
                inLen: encryptedKey.CipherData.CipherValue.Length);
        }

        private OaepEncoding CreateDecryptEncoding(EncryptedKey encryptedKey)
        {
            string xpath = $"//*[local-name()='DigestMethod' and namespace-uri()='{Constants.Namespaces.XmlDsig}']";
            var digestMethodNode = encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            IDigest digestMethod = RetrieveDigestMethod(digestMethodNode);
            var engine = new RsaEngine();

            return new OaepEncoding(engine, digestMethod, new Sha1Digest(), new byte[0]);
        }

        private IDigest RetrieveDigestMethod(XmlElement digestMethodNode)
        {
            IDigest digestMethod = new Sha256Digest();
            string algorithm = digestMethodNode?.GetAttribute("Algorithm");

            if (algorithm != null)
                digestMethod = DigestUtilities.GetDigest(algorithm.Substring(algorithm.IndexOf('#') + 1));

            return digestMethod;
        }
    }
}