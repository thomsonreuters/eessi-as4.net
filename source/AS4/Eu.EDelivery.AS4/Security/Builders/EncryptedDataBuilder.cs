using System;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Transforms;

namespace Eu.EDelivery.AS4.Security.Builders
{
    /// <summary>
    /// Builder to create <see cref="EncryptedData"/> Models
    /// </summary>
    internal class EncryptedDataBuilder
    {
        private DataEncryptionConfiguration _data;
        private string _mimeType;
        private string _uri;
        private AS4EncryptedKey _encryptionKey;

    
        public EncryptedDataBuilder WithEncryptionKey(AS4EncryptedKey encryptionKey)
        {
            _encryptionKey = encryptionKey;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="uri"/> to the <see cref="EncryptedData"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public EncryptedDataBuilder WithUri(string uri)
        {
            _uri = uri;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="mimeType"/> to the <see cref="EncryptedData"/>
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public EncryptedDataBuilder WithMimeType(string mimeType)
        {
            _mimeType = mimeType;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="data"/> to the <see cref="EncryptedData"/>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public EncryptedDataBuilder WithDataEncryptionConfiguration(DataEncryptionConfiguration data)
        {
            _data = data;
            return this;
        }

        /// <summary>
        /// Build the <see cref="EncryptedData"/> Model
        /// </summary>
        /// <returns></returns>
        public EncryptedData Build()
        {
            EncryptedData encryptedData = CreateEncryptedData();
            AssembleEncryptedData(encryptedData);

            return encryptedData;
        }

        private EncryptedData CreateEncryptedData()
        {
            return new EncryptedData
            {
                Id = "ed-" + Guid.NewGuid(),
                Type = _data.EncryptionType,
                EncryptionMethod = new EncryptionMethod(_data.EncryptionMethod),
                CipherData = new CipherData(),
                MimeType = _mimeType
            };
        }

        private void AssembleEncryptedData(EncryptedData encryptedData)
        {
            encryptedData.CipherData.CipherReference = new CipherReference("cid:" + _uri);
            encryptedData.CipherData.CipherReference.TransformChain.Add(new AttachmentCiphertextTransform());

            if (_encryptionKey != null)
            {
                encryptedData.KeyInfo.AddClause(new ReferenceSecurityTokenReference(_encryptionKey.GetReferenceId()));
            }            
        }
    }
}