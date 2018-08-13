using System;
using Eu.EDelivery.AS4.Security.Strategies;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace Eu.EDelivery.AS4.Security.Factories
{
    /// <summary>
    /// Factory to create <see cref="OaepEncoding"/> Encoding
    /// </summary>
    internal class EncodingFactory
    {
        public static EncodingFactory Instance = new EncodingFactory();

        private EncodingFactory() { }

        /// <summary>
        /// Create and <see cref="OaepEncoding"/> instance
        /// </summary>
        /// <param name="digestAlgorithm"></param>
        /// <param name="mgfAlgorithm"></param>
        /// <returns></returns>
        public OaepEncoding Create(string digestAlgorithm = CryptoStrategy.XmlEncSHA1Url, string mgfAlgorithm = null)
        {
            IDigest digest = GetDigestForAlgorithm(digestAlgorithm);
            IDigest mgf = GetMgfForAlgorithm(mgfAlgorithm);

            return new OaepEncoding(
                cipher: new RsaEngine(),                
                hash: digest,
                mgf1Hash: mgf,
                encodingParams: new byte[0]);
        }

        private static IDigest GetDigestForAlgorithm(string algorithm)
        {
            if (string.IsNullOrWhiteSpace(algorithm))
            {
                return new Sha1Digest();
            }

            return DigestUtilities.GetDigest(algorithm.Substring(algorithm.IndexOf('#') + 1));
        }

        private static IDigest GetMgfForAlgorithm(string algorithm)
        {
            if (string.IsNullOrWhiteSpace(algorithm))
            {
                return new Sha1Digest();
            }

            int startIndex = algorithm.IndexOf("#mgf1", StringComparison.OrdinalIgnoreCase) + 5;
            if (startIndex > algorithm.Length)
            {
                return new Sha1Digest();
            }

            return DigestUtilities.GetDigest(algorithm.Substring(startIndex));
        }
    }
}