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

        private EncodingFactory() {}

        /// <summary>
        /// Create and <see cref="OaepEncoding"/> instance
        /// </summary>
        /// <param name="digestAlgorithm"></param>
        /// <returns></returns>
        public OaepEncoding Create(string digestAlgorithm = null)
        {
            IDigest digest = digestAlgorithm != null ? GetDigest(digestAlgorithm) : new Sha1Digest();
            return CreateEncoding(digest);
        }

        private static IDigest GetDigest(string algorithm)
        {
            return DigestUtilities.GetDigest(algorithm.Substring(algorithm.IndexOf('#') + 1));
        }

        private static OaepEncoding CreateEncoding(IDigest digest)
        {
            return new OaepEncoding(
                cipher: new RsaEngine(),
                hash: digest,
                mgf1Hash: new Sha1Digest(),
                encodingParams: new byte[0]);
        }
    }
}