using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.Transforms;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    public abstract class CryptoStrategy : EncryptedXml
    {
        public const string XmlEncRSAOAEPUrlWithMgf = "http://www.w3.org/2009/xmlenc11#rsa-oaep";
        public const string XmlEncSHA1Url = "http://www.w3.org/2000/09/xmldsig#sha1";

        static CryptoStrategy()
        {
            CryptoConfig.AddAlgorithm(typeof(AttachmentCiphertextTransform), AttachmentCiphertextTransform.Url);
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes128-gcm");
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes192-gcm");
            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), "http://www.w3.org/2009/xmlenc11#aes256-gcm");
        }

        protected static SymmetricAlgorithm CreateSymmetricAlgorithm(string name, byte[] key)
        {
            var symmetricAlgorithm = (SymmetricAlgorithm)CryptoConfig.CreateFromName(name);
            symmetricAlgorithm.Key = key;

            return symmetricAlgorithm;
        }

        protected static VirtualStream CreateVirtualStreamOf(Stream innerStream)
        {
            return VirtualStream.CreateVirtualStream(
                expectedSize: innerStream.CanSeek ? innerStream.Length : VirtualStream.ThresholdMax);
        }
    }
}