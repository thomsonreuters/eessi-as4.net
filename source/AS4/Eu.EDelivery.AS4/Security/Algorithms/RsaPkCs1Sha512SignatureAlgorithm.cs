using System.Security.Cryptography;

namespace Eu.EDelivery.AS4.Security.Algorithms
{
    /// <summary>
    /// Declare the signature type for rsa-sha512
    /// </summary>
    public class RsaPkCs1Sha512SignatureAlgorithm : SignatureAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RsaPkCs1Sha512SignatureAlgorithm"/> class. 
        /// Create a new RSA SHA 384 Algorithm with 
        /// setted Key/Digest/(De)Formatter Algorithms
        /// </summary>
        public RsaPkCs1Sha512SignatureAlgorithm()
        {
            this.KeyAlgorithm = typeof(RSACryptoServiceProvider).FullName;
            this.DigestAlgorithm = typeof(SHA512CryptoServiceProvider).FullName;
            this.FormatterAlgorithm = typeof(RSAPKCS1SignatureFormatter).FullName;
            this.DeformatterAlgorithm = typeof(RSAPKCS1SignatureDeformatter).FullName;
        }

        /// <summary>
        /// Get the Identifier of the Signature Algorithm
        /// </summary>
        /// <returns></returns>
        public override string GetIdentifier()
        {
            return "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";
        }

        /// <summary>
        /// Create an <see cref="AsymmetricSignatureDeformatter"/> from a <see cref="AsymmetricAlgorithm"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
        {
            var sigProcessor = (AsymmetricSignatureDeformatter)CryptoConfig.CreateFromName(DeformatterAlgorithm);
            sigProcessor.SetKey(key);
            sigProcessor.SetHashAlgorithm("SHA512");

            return sigProcessor;
        }

        public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            var sigProcessor =
                (AsymmetricSignatureFormatter)CryptoConfig.CreateFromName(FormatterAlgorithm);
            sigProcessor.SetKey(key);
            sigProcessor.SetHashAlgorithm("SHA512");

            return sigProcessor;
        }
    }
}
