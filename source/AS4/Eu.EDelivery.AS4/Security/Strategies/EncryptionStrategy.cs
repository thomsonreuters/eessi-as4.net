using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Builders;
using Eu.EDelivery.AS4.Security.Encryption;
using MimeKit.IO;
using NLog;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    /// <summary>
    /// An <see cref="IEncryptionStrategy"/> implementation
    /// responsible for the Encryption of the <see cref="AS4Message"/>
    /// </summary>
    internal class EncryptionStrategy : CryptoStrategy, IEncryptionStrategy
    {
        private readonly List<Attachment> _attachments;

        private readonly KeyEncryptionConfiguration _keyEncryptionConfig;
        private readonly DataEncryptionConfiguration _dataEncryptionConfig;


        private readonly List<EncryptedData> _encryptedDatas = new List<EncryptedData>();

        private AS4EncryptedKey _as4EncryptedKey;

        internal EncryptionStrategy(
            KeyEncryptionConfiguration keyEncryptionConfig,
            DataEncryptionConfiguration dataEncryptionConfig,
            IEnumerable<Attachment> attachments)
        {
            _keyEncryptionConfig = keyEncryptionConfig;
            _dataEncryptionConfig = dataEncryptionConfig;
            _attachments = attachments.ToList();
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
            if (_as4EncryptedKey != null)
            {
                if (_as4EncryptedKey.SecurityTokenReference != null)
                {
                    _as4EncryptedKey.SecurityTokenReference.AppendSecurityTokenTo(securityElement, securityDocument);
                }
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
            AS4EncryptedKey as4EncryptedKey = GetEncryptedKey(encryptionKey, _keyEncryptionConfig);

            _as4EncryptedKey = as4EncryptedKey;

            using (SymmetricAlgorithm encryptionAlgorithm =
                CreateSymmetricAlgorithm(_dataEncryptionConfig.EncryptionMethod, encryptionKey))
            {
                EncryptAttachmentsWithAlgorithm(as4EncryptedKey, encryptionAlgorithm);
            }
        }

        private static byte[] GenerateSymmetricKey(int keySize)
        {
            using (var rijn = new RijndaelManaged { KeySize = keySize })
            {
                return rijn.Key;
            }
        }

        private static AS4EncryptedKey GetEncryptedKey(
            byte[] symmetricKey,
            KeyEncryptionConfiguration keyEncryptionConfig)
        {
            return
                AS4EncryptedKey.CreateEncryptedKeyBuilderForKey(symmetricKey, keyEncryptionConfig)
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
                .WithEncryptionKey(encryptedKey)
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
    }
}