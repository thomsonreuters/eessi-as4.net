using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Factories;
using Eu.EDelivery.AS4.Security.Serializers;
using Eu.EDelivery.AS4.Streaming;
using MimeKit;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Security;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    internal class DecryptionStrategy : CryptoStrategy
    {
        private readonly XmlDocument _soapEnvelope;
        private readonly IEnumerable<Attachment> _attachments;
        private readonly X509Certificate2 _certificate;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DecryptionStrategy"/> class.
        /// </summary>
        internal DecryptionStrategy(XmlDocument envelopeDocument, IEnumerable<Attachment> attachments, X509Certificate2 certificate)
        {
            _soapEnvelope = envelopeDocument;
            _attachments = attachments;
            _certificate = certificate;
        }

        /// <summary>
        /// Decrypts the <see cref="AS4Message"/>, replacing the encrypted content with the decrypted content.
        /// </summary>
        public void DecryptMessage()
        {
            IEnumerable<EncryptedData> encryptedDatas =
                new EncryptedDataSerializer(_soapEnvelope).SerializeEncryptedDatas();

            var as4EncryptedKey = AS4EncryptedKey.LoadFromXmlDocument(_soapEnvelope);

            byte[] key = DecryptEncryptedKey(as4EncryptedKey, _certificate);

            foreach (EncryptedData encryptedData in encryptedDatas)
            {
                DecryptEncryptedData(encryptedData, key);
            }
        }

        private static byte[] DecryptEncryptedKey(AS4EncryptedKey encryptedKey, X509Certificate2 certificate)
        {
            OaepEncoding encoding = EncodingFactory.Instance
                                                   .Create(encryptedKey.GetDigestAlgorithm(), encryptedKey.GetMaskGenerationFunction());

            // We do not look at the KeyInfo element in here, but rather decrypt it with the certificate provided as argument.
            // Call GetRSAPrivateKey to avoid KeySet does not exist exceptions that might be thrown.
            RSA privateKey = certificate.GetRSAPrivateKey();

            if (privateKey == null)
            {
                throw new CryptographicException("The decryption certificate does not contain a private key.");
            }

            AsymmetricCipherKeyPair encryptionCertificateKeyPair =
                DotNetUtilities.GetRsaKeyPair(privateKey);

            encoding.Init(false, encryptionCertificateKeyPair.Private);

            CipherData cipherData = encryptedKey.GetCipherData();
            return encoding.ProcessBlock(
                inBytes: cipherData.CipherValue, inOff: 0, inLen: cipherData.CipherValue.Length);
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
            if (_attachments == null)
            {
                return;
            }

            string uri = encryptedData.CipherData.CipherReference.Uri;
            Attachment attachment = _attachments.SingleOrDefault(x => string.Equals(x.Id, uri.Substring(4)));
            if (attachment != null)
            {
                Stream decryptedStream = DecryptData(encryptedData, attachment.Content, decryptAlgorithm);

                var transformer = AttachmentTransformer.Create(encryptedData.Type);
                transformer.Transform(attachment, decryptedStream);

                attachment.ContentType = encryptedData.MimeType;
            }
            else
            {
                string description = $"Attachment {uri.Substring(4)} cannot be found and can therefore not be decrypted";
                Logger.Error(description);

                throw new CryptographicException(description);
            }
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