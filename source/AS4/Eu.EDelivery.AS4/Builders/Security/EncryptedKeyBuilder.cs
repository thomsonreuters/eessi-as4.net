using System;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.References;
using Org.BouncyCastle.Crypto.Encodings;

namespace Eu.EDelivery.AS4.Builders.Security
{
    /// <summary>
    /// Builder to create <see cref="EncryptedKey"/> Models
    /// </summary>
    public class EncryptedKeyBuilder
    {
        private byte[] _symmetricKey;
        private OaepEncoding _encoding;
        private SecurityTokenReference _securityTokenReference;

        /// <summary>
        /// Add a <paramref name="symmetricKey"/> to the <see cref="EncryptedKey"/>
        /// </summary>
        /// <param name="symmetricKey"></param>
        /// <returns></returns>
        public EncryptedKeyBuilder WithSymmetricKey(byte[] symmetricKey)
        {
            this._symmetricKey = symmetricKey;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="encoding"/> to the <see cref="EncryptedKey"/>
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public EncryptedKeyBuilder WithEncoding(OaepEncoding encoding)
        {
            this._encoding = encoding;
            return this;
        }

        /// <summary>
        /// Add a <paramref name="reference"/> to the <see cref="EncryptedKey"/>
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public EncryptedKeyBuilder WithSecurityTokenReference(SecurityTokenReference reference)
        {
            this._securityTokenReference = reference;
            return this;
        }

        /// <summary>
        /// Build the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public EncryptedKey Build()
        {
            EncryptedKey encryptedKey = CreateDefaultEncryptedKey();
            encryptedKey.KeyInfo.AddClause(this._securityTokenReference);
           
            return encryptedKey;
        }

        private EncryptedKey CreateDefaultEncryptedKey()
        {
            return new EncryptedKey
            {
                Id = "ek-" + Guid.NewGuid(),
                EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSAOAEPUrl),
                CipherData = new CipherData
                {
                    CipherValue = this._encoding
                    .ProcessBlock(this._symmetricKey, inOff: 0, inLen: this._symmetricKey.Length)
                }
            };
        }
    }
}
