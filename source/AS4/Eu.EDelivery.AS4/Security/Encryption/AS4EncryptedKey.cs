using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Security.Factories;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;


namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// AS4 Implementation to wrap the <see cref="EncryptedKey"/>
    /// </summary>
    public class AS4EncryptedKey
    {
        #region Builder
        internal class EncryptedKeyBuilder
        {
            private readonly X509Certificate2 _certificate;
            private byte[] _key;
            private string _algorithmUri = EncryptionStrategy.XmlEncRSAOAEPUrlWithMgf;
            private string _digestAlgorithmUri = EncryptionStrategy.XmlEncSHA1Url;
            private string _mgfAlgorithmUri = null;
            private SecurityTokenReference _securityTokenReference;

            private EncryptedKeyBuilder(X509Certificate2 certificate, byte[] key)
            {
                _certificate = certificate;
                _key = key;
            }

            public static EncryptedKeyBuilder ForKey(byte[] symmetricKey, X509Certificate2 certificate)
            {
                return new EncryptedKeyBuilder(certificate, symmetricKey);
            }

            public EncryptedKeyBuilder WithEncryptionMethod(string algorithmUri)
            {
                _algorithmUri = algorithmUri;
                return this;
            }

            public EncryptedKeyBuilder WithDigest(string algorithmUri)
            {
                _digestAlgorithmUri = algorithmUri;
                return this;
            }

            public EncryptedKeyBuilder WithMgf(string mgfAlgorithmUri)
            {
                _mgfAlgorithmUri = mgfAlgorithmUri;
                return this;
            }

            public EncryptedKeyBuilder WithSecurityTokenReference(SecurityTokenReference reference)
            {
                this._securityTokenReference = reference;
                return this;
            }

            public AS4EncryptedKey Build()
            {
                return new AS4EncryptedKey(BuildEncryptedKey(), _digestAlgorithmUri, _mgfAlgorithmUri);
            }

            private EncryptedKey BuildEncryptedKey()
            {
                var encoding = EncodingFactory.Instance.Create(digestAlgorithm: _digestAlgorithmUri, mgfAlgorithm: _mgfAlgorithmUri);

                RSA rsaPublicKey = _certificate.GetRSAPublicKey();
                RsaKeyParameters publicKey = DotNetUtilities.GetRsaPublicKey(rsaPublicKey);
                encoding.Init(forEncryption: true, param: publicKey);

                var encryptedKey = new EncryptedKey
                {
                    Id = "ek-" + Guid.NewGuid(),
                    EncryptionMethod = new EncryptionMethod(_algorithmUri),
                    CipherData = new CipherData
                    {
                        CipherValue = encoding.ProcessBlock(_key, inOff: 0, inLen: _key.Length)
                    }
                };

                if (_securityTokenReference != null)
                {
                    encryptedKey.KeyInfo.AddClause(this._securityTokenReference);
                }

                _key = null;

                return encryptedKey;
            }
        }

        #endregion

        private readonly EncryptedKey _encryptedKey;
        private readonly string _digestAlgorithm;
        private readonly string _mgfAlgorithm;

        private AS4EncryptedKey(EncryptedKey encryptedKey, string digestAlgorithm, string mgfAlgorithm)
        {
            _encryptedKey = encryptedKey;
            _digestAlgorithm = digestAlgorithm;
            _mgfAlgorithm = mgfAlgorithm;
        }

        /// <summary>
        /// Loads an AS4EncryptedKey instance from the given XmlDocument
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        internal static AS4EncryptedKey LoadFromXmlDocument(XmlDocument xmlDocument)
        {
            var encryptedKeyElement = xmlDocument.SelectSingleNode("//*[local-name()='EncryptedKey']") as XmlElement;

            if (encryptedKeyElement == null)
            {
                throw new ArgumentException(@"No EncryptedKey element found in XmlDocument", nameof(xmlDocument));
            }

            var encryptedKey = new EncryptedKey();

            encryptedKey.LoadXml(encryptedKeyElement);

            return new AS4EncryptedKey(encryptedKey, GetDigestAlgorithm(encryptedKey), GetMgfAlgorithm(encryptedKey));
        }

        /// <summary>
        /// Initializes an <see cref="EncryptedKeyBuilder"/> instance which can be used to instantiate an AS4EncryptedKey instance.
        /// </summary>
        /// <param name="symmetricKey">The encryption key that should be encrypted.</param>
        /// <param name="certificate">The certificate that should be used for encryption.</param>
        /// <returns></returns>
        internal static EncryptedKeyBuilder CreateEncryptedKeyBuilderForKey(byte[] symmetricKey, X509Certificate2 certificate)
        {
            return EncryptedKeyBuilder.ForKey(symmetricKey, certificate);
        }

        /// <summary>
        /// Creates an AS4EncryptedKey instance for the given <paramref name="encryptedKey"/>
        /// </summary>
        /// <remarks>This method should only be used for testing purposes.</remarks>
        /// <param name="encryptedKey"></param>
        /// <returns></returns>
        internal static AS4EncryptedKey FromEncryptedKey(EncryptedKey encryptedKey)
        {
            return new AS4EncryptedKey(encryptedKey, EncryptionStrategy.XmlEncSHA1Url, null);
        }

        /// <summary>
        /// Add a <see cref="DataReference"/> with a given <paramref name="uri"/>
        /// </summary>
        /// <param name="uri"></param>
        public void AddDataReference(string uri)
        {
            this._encryptedKey.ReferenceList.Add(new DataReference("#" + uri));
        }

        /// <summary>
        /// Get the Reference Id from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetReferenceId()
        {
            return _encryptedKey.Id;
        }

        /// <summary>
        /// Get the <see cref="CipherData"/> from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public CipherData GetCipherData()
        {
            return this._encryptedKey.CipherData;
        }

        private static string GetDigestAlgorithm(EncryptedKey encryptedKey)
        {
            string xpath = $".//*[local-name()='EncryptionMethod']/*[local-name()='DigestMethod' and namespace-uri()='{Constants.Namespaces.XmlDsig}']";
            var digestNode = encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return digestNode?.GetAttribute("Algorithm");
        }

        private static string GetMgfAlgorithm(EncryptedKey encryptedKey)
        {
            string xpath = $".//*[local-name()='EncryptionMethod']/*[local-name()='MGF']";

            var node = encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return node?.GetAttribute("Algorithm");
        }

        public string GetEncryptionAlgorithm()
        {
            string xpath = $".//*[local-name()='EncryptionMethod']";

            var node = _encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return node?.GetAttribute("Algorithm");
        }

        /// <summary>
        /// Get the DigestMethod Element from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetDigestAlgorithm()
        {
            return _digestAlgorithm;
        }

        /// <summary>
        /// Get the MGF (Mask Generation Function) from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetMaskGenerationFunction()
        {
            return _mgfAlgorithm;
        }

        /// <summary>
        /// Append the <EncryptedKey/> Element to a given <paramref name="securityElement"/>
        /// </summary>
        /// <param name="securityElement"></param>
        public void AppendEncryptedKey(XmlElement securityElement)
        {
            XmlElement encryptedKeyElement = GetEncryptedKeyElement();
            XmlNode importedEncryptedKeyNode = securityElement.OwnerDocument.ImportNode(encryptedKeyElement, deep: true);

            securityElement.AppendChild(importedEncryptedKeyNode);
        }

        private XmlElement GetEncryptedKeyElement()
        {
            XmlElement encryptedKeyElement = this._encryptedKey.GetXml();

            var encryptionMethodNode = encryptedKeyElement.SelectSingleNode("//*[local-name()='EncryptionMethod']");

            if (encryptionMethodNode != null)
            {
                AppendDigestMethod(encryptionMethodNode, _digestAlgorithm);

                if (_mgfAlgorithm != null)
                {
                    AppendMgfMethod(encryptionMethodNode, _mgfAlgorithm);
                }
            }

            return encryptedKeyElement;
        }

        private static void AppendDigestMethod(XmlNode encryptionMethodNode, string digestAlgorithm)
        {
            XmlElement digestMethod = encryptionMethodNode.OwnerDocument
                .CreateElement("DigestMethod", Constants.Namespaces.XmlDsig);

            digestMethod.SetAttribute("Algorithm", digestAlgorithm);

            encryptionMethodNode.AppendChild(digestMethod);
        }

        private static void AppendMgfMethod(XmlNode node, string mgfAlgorithm)
        {
            var mgfElement = node.OwnerDocument.CreateElement("MGF", Constants.Namespaces.XmlEnc11);
            mgfElement.SetAttribute("Algorithm", mgfAlgorithm);
            node.AppendChild(mgfElement);
        }
    }
}
