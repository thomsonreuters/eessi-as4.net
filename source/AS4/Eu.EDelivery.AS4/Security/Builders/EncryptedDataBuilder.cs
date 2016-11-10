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
        private string _referenceId;

        /// <summary>
        /// Add a <paramref name="referenceId"/> to the <see cref="EncryptedData"/>
        /// </summary>
        /// <param name="referenceId"></param>
        /// <returns></returns>
        public EncryptedDataBuilder WithReferenceId(string referenceId)
        {
            this._referenceId = referenceId;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="uri"/> to the <see cref="EncryptedData"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public EncryptedDataBuilder WithUri(string uri)
        {
            this._uri = uri;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="mimeType"/> to the <see cref="EncryptedData"/>
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        public EncryptedDataBuilder WithMimeType(string mimeType)
        {
            this._mimeType = mimeType;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="data"/> to the <see cref="EncryptedData"/>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public EncryptedDataBuilder WithDataEncryptionConfiguration(DataEncryptionConfiguration data)
        {
            this._data = data;
            return this;
        }

        /// <summary>
        /// Build the <see cref="EncryptedData"/> Model
        /// </summary>
        /// <returns></returns>
        public EncryptedData Build()
        {
            EncryptedData encryptedData = CreateEncryptedData();
            AssemblyEncryptedData(encryptedData);

            return encryptedData;
        }

        private EncryptedData CreateEncryptedData()
        {
            return new EncryptedData
            {
                Id = "ed-" + Guid.NewGuid(),
                Type = this._data.EncryptionType,
                EncryptionMethod = new EncryptionMethod(this._data.EncryptionMethod),
                CipherData = new CipherData(),
                MimeType = this._mimeType
            };
        }

        private void AssemblyEncryptedData(EncryptedData encryptedData)
        {
            encryptedData.CipherData.CipherReference = new CipherReference("cid:" + this._uri);
            encryptedData.CipherData.CipherReference.TransformChain.Add(new AttachmentCiphertextTransform());
            encryptedData.KeyInfo.AddClause(new ReferenceSecurityTokenReference {ReferenceId = this._referenceId});
        }
    }
}