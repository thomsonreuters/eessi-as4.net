using System.Security.Cryptography;

namespace Eu.EDelivery.AS4.Security.Algorithms
{
    /// <summary>
    /// Declare the signature type for rsa-sha384
    /// </summary>
    public class RsaPkCs1Sha384SignatureDescription : SignatureAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RsaPkCs1Sha384SignatureDescription"/> class. 
        /// Create a new RSA SHA 384 Algorithm with 
        /// setted Key/Digest/(De)Formatter Algorithms
        /// </summary>
        public RsaPkCs1Sha384SignatureDescription()
        {
            this.KeyAlgorithm = typeof(RSACryptoServiceProvider).FullName;
            this.DigestAlgorithm = typeof(SHA384CryptoServiceProvider).FullName;
            this.FormatterAlgorithm = typeof(RSAPKCS1SignatureFormatter).FullName;
            this.DeformatterAlgorithm = typeof(RSAPKCS1SignatureDeformatter).FullName;
        }

        /// <summary>
        /// Get the Identifier of the Signature Algorithm
        /// </summary>
        /// <returns></returns>
        public override string GetIdentifier()
        {
            return "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";
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
            sigProcessor.SetHashAlgorithm("SHA384");

            return sigProcessor;
        }

        public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
        {
            var sigProcessor =
                (AsymmetricSignatureFormatter)CryptoConfig.CreateFromName(FormatterAlgorithm);
            sigProcessor.SetKey(key);
            sigProcessor.SetHashAlgorithm("SHA384");

            return sigProcessor;
        }
    }

}
