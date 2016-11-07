using System.Collections.Generic;
using System.Xml;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Extensions;

namespace Eu.EDelivery.AS4.Security.Algorithms
{
    /// <summary>
    /// Class to provide <see cref="SignatureAlgorithm"/> implementations
    /// </summary>
    public class SignatureAlgorithmProvider : ISignatureAlgorithmProvider
    {
        private readonly IDictionary<string, SignatureAlgorithm> _algorithms;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignatureAlgorithmProvider"/> class. 
        /// Create a new Signature Algorithm provider
        /// with Defaults registered
        /// </summary>
        public SignatureAlgorithmProvider()
        {
            this._algorithms = new Dictionary<string, SignatureAlgorithm>
            {
                ["http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"] = new RsaPkCs1Sha256SignatureAlgorithm(),
                ["http://www.w3.org/2001/04/xmldsig-more#rsa-sha384"] = new RsaPkCs1Sha384SignatureDescription(),
                ["http://www.w3.org/2001/04/xmldsig-more#rsa-sha512"] = new RsaPkCs1Sha512SignatureAlgorithm(),
            };
        }

        /// <summary>
        /// Get a <see cref="SignatureAlgorithm"/> implementation
        /// based on a algorithm namespace
        /// </summary>
        /// <param name="algorithmNamespace"></param>
        /// <returns></returns>
        public SignatureAlgorithm Get(string algorithmNamespace)
        {
            return this._algorithms.ReadMandatoryProperty(algorithmNamespace);
        }

        public SignatureAlgorithm Get(XmlDocument envelopeDocument)
        {
            XmlElement xmlSignatureElement = GetSignatureElement(envelopeDocument);
            string algorithmAttribute = GetSignatureAlgorithm(xmlSignatureElement);

            return this._algorithms[algorithmAttribute];
        }

        private XmlElement GetSignatureElement(XmlDocument envelopeDocument)
        {
            var xmlSignatureElement = envelopeDocument
                .SelectSingleNode("//*[local-name()='SignatureMethod']") as XmlElement;

            if (xmlSignatureElement == null)
                throw new AS4Exception("No SignatureMethod XmlElement found in given Envelope Document");

            return xmlSignatureElement;
        }

        private string GetSignatureAlgorithm(XmlElement xmlSignatureElement)
        {
            string algorithmAttribute = xmlSignatureElement.GetAttribute("Algorithm");

            if (!this._algorithms.ContainsKey(algorithmAttribute))
                throw new AS4Exception($"No given Signature Algorithm found for: {algorithmAttribute}");

            return algorithmAttribute;
        }
    }

    /// <summary>
    /// Interface used for Testing
    /// </summary>
    public interface ISignatureAlgorithmProvider
    {
        SignatureAlgorithm Get(string algorithmNamespace);
        SignatureAlgorithm Get(XmlDocument envelopeDocument);
    }
}