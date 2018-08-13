using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Security.References;

namespace Eu.EDelivery.AS4.Security.Signing
{
    public class CalculateSignatureConfig
    {
        public X509Certificate2 SigningCertificate { get; }
        public X509ReferenceType ReferenceTokenType { get; }
        public string SigningAlgorithm { get; }
        public string HashFunction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateSignatureConfig"/> class.
        /// </summary>
        public CalculateSignatureConfig(X509Certificate2 signingCertificate)
            : this (signingCertificate,
                    X509ReferenceType.BSTReference,
                    Constants.SignAlgorithms.Sha256,
                    Constants.HashFunctions.Sha256
            )
        {
        }

        public CalculateSignatureConfig(X509Certificate2 signingCertificate,
                                        X509ReferenceType referenceTokenType,
                                        string signingAlgorithm,
                                        string hashFunction)
        {
            SigningCertificate = signingCertificate;
            ReferenceTokenType = referenceTokenType;
            SigningAlgorithm = signingAlgorithm;
            HashFunction = hashFunction;
        }
    }
}