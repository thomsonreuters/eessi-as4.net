using System;
using Org.BouncyCastle.Asn1.Pkcs;
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
        public OaepEncoding Create(string digestAlgorithm = null, string mgfAlgorithm = null)
        {
            var digest = !string.IsNullOrWhiteSpace(digestAlgorithm) ? GetDigest(digestAlgorithm) : new Sha1Digest();
            var mgf = !string.IsNullOrWhiteSpace(mgfAlgorithm) ? GetMgf(mgfAlgorithm) : new Sha1Digest();

            return CreateEncoding(digest, mgf);
        }

        private static IDigest GetDigest(string algorithm)
        {
            return DigestUtilities.GetDigest(algorithm.Substring(algorithm.IndexOf('#') + 1));
        }

        private static IDigest GetMgf(string algorithm)
        {
            int startIndex = algorithm.IndexOf("#mgf1", StringComparison.OrdinalIgnoreCase) + 5;

            if (startIndex > algorithm.Length)
            {
                return new Sha1Digest();
            }

            return DigestUtilities.GetDigest(algorithm.Substring(startIndex));
        }

        private static OaepEncoding CreateEncoding(IDigest digest, IDigest mgf)
        {         
            return new OaepEncoding(
                cipher: new RsaEngine(),                
                hash: digest,
                mgf1Hash: mgf,
                encodingParams: new byte[0]);
        }
    }
}