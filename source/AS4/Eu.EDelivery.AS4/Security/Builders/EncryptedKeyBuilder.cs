using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.Factories;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Eu.EDelivery.AS4.Security.Builders
{
    /// <summary>
    /// Builder to create <see cref="EncryptedKey"/> Models
    /// </summary>
    //[Obsolete]
    //internal class EncryptedKeyBuilder
    //{
    //    private byte[] _symmetricKey;
    //    private OaepEncoding _encoding;
    //    private SecurityTokenReference _securityTokenReference;

    //    /// <summary>
    //    /// Add a <paramref name="symmetricKey"/> to the <see cref="EncryptedKey"/>
    //    /// </summary>
    //    /// <param name="symmetricKey"></param>
    //    /// <returns></returns>
    //    public EncryptedKeyBuilder WithSymmetricKey(byte[] symmetricKey)
    //    {
    //        this._symmetricKey = symmetricKey;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Add a <paramref name="encoding"/> to the <see cref="EncryptedKey"/>
    //    /// </summary>
    //    /// <param name="encoding"></param>
    //    /// <returns></returns>
    //    public EncryptedKeyBuilder WithEncoding(OaepEncoding encoding)
    //    {
    //        this._encoding = encoding;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Add a <paramref name="reference"/> to the <see cref="EncryptedKey"/>
    //    /// </summary>
    //    /// <param name="reference"></param>
    //    /// <returns></returns>
    //    public EncryptedKeyBuilder WithSecurityTokenReference(SecurityTokenReference reference)
    //    {
    //        this._securityTokenReference = reference;
    //        return this;
    //    }

    //    /// <summary>
    //    /// Build the <see cref="EncryptedKey"/>
    //    /// </summary>
    //    /// <returns></returns>
    //    public EncryptedKey Build()
    //    {
    //        EncryptedKey encryptedKey = CreateDefaultEncryptedKey();
    //        encryptedKey.KeyInfo.AddClause(this._securityTokenReference);

    //        return encryptedKey;
    //    }

    //    private EncryptedKey CreateDefaultEncryptedKey()
    //    {
    //        return new EncryptedKey
    //        {
    //            Id = "ek-" + Guid.NewGuid(),
    //            EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSAOAEPUrl),
    //            CipherData = new CipherData
    //            {
    //                CipherValue = this._encoding
    //                .ProcessBlock(this._symmetricKey, inOff: 0, inLen: this._symmetricKey.Length)
    //            }
    //        };
    //    }
    //}

    ////internal class EncryptedKeyBuilder
    ////{
    ////    private readonly X509Certificate2 _certificate;
    ////    private byte[] _key;
    ////    private string _algorithmUri = EncryptedXml.XmlEncRSAOAEPUrl;
    ////    private string _digestAlgorithmUri = EncryptionStrategy.XmlEncSHA1Url;
    ////    private string _mgfAlgorithmUri = null;
    ////    private SecurityTokenReference _securityTokenReference;

    ////    private EncryptedKeyBuilder(X509Certificate2 certificate, byte[] key)
    ////    {
    ////        _certificate = certificate;
    ////        _key = key;
    ////    }

    ////    public static EncryptedKeyBuilder ForKey(byte[] symmetricKey, X509Certificate2 certificate)
    ////    {
    ////        return new EncryptedKeyBuilder(certificate, symmetricKey);
    ////    }

    ////    public EncryptedKeyBuilder WithEncryptionMethod(string algorithmUri)
    ////    {
    ////        _algorithmUri = algorithmUri;
    ////        return this;
    ////    }

    ////    public EncryptedKeyBuilder WithDigest(string algorithmUri)
    ////    {
    ////        _digestAlgorithmUri = algorithmUri;
    ////        return this;
    ////    }

    ////    public EncryptedKeyBuilder WithMgf(string mgfAlgorithmUri)
    ////    {
    ////        _mgfAlgorithmUri = mgfAlgorithmUri;
    ////        return this;
    ////    }

    ////    public EncryptedKeyBuilder WithSecurityTokenReference(SecurityTokenReference reference)
    ////    {
    ////        this._securityTokenReference = reference;
    ////        return this;
    ////    }

    ////    public AS4EncryptedKey Build()
    ////    {
    ////        return new AS4EncryptedKey(BuildEncryptedKey(), _digestAlgorithmUri, _mgfAlgorithmUri);
    ////    }

    ////    private EncryptedKey BuildEncryptedKey()
    ////    {
    ////        var encoding = EncodingFactory.Instance.Create(digestAlgorithm: _digestAlgorithmUri, mgfAlgorithm: _mgfAlgorithmUri);

    ////        RSA rsaPublicKey = _certificate.GetRSAPublicKey();
    ////        RsaKeyParameters publicKey = DotNetUtilities.GetRsaPublicKey(rsaPublicKey);
    ////        encoding.Init(forEncryption: true, param: publicKey);
            
    ////        var encryptedKey = new EncryptedKey
    ////        {
    ////            Id = "ek-" + Guid.NewGuid(),
    ////            EncryptionMethod = new EncryptionMethod(_algorithmUri),
    ////            CipherData = new CipherData
    ////            {
    ////                CipherValue = encoding.ProcessBlock(_key, inOff: 0, inLen: _key.Length)
    ////            }
    ////        };

    ////        if (_securityTokenReference != null)
    ////        {
    ////            encryptedKey.KeyInfo.AddClause(this._securityTokenReference);
    ////        }

    ////        _key = null;

    ////        return encryptedKey;
    ////    }


    ////}
    
}
