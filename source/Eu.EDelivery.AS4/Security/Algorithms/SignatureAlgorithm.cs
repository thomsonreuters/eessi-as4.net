using System.Security.Cryptography;

namespace Eu.EDelivery.AS4.Security.Algorithms
{
    /// <summary>
    /// Abstract Class to have 
    /// </summary>
    public abstract class SignatureAlgorithm : SignatureDescription
    {
        /// <summary>
        /// Get the Identifier of the Signature Algorithm
        /// </summary>
        /// <returns></returns>
        public abstract string GetIdentifier();

        /// <summary>
        /// Create an <see cref="AsymmetricSignatureDeformatter"/> from a <see cref="AsymmetricAlgorithm"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key);
    }
}