using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;

namespace Eu.EDelivery.AS4.Security.Repositories
{
    /// <summary>
    /// Respository to navigate the Reference ID Xml Elements
    /// </summary>
    internal class SignedXmlRepository
    {
        private readonly string[] _allowedIdNodeNames;
        private readonly XmlDocument _document;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignedXmlRepository" /> class
        /// </summary>
        /// <param name="document"></param>
        public SignedXmlRepository(XmlDocument document)
        {
            _document = document;
            _allowedIdNodeNames = new[] {"Id", "id", "ID"};
        }

        /// <summary>
        /// Get the <see cref="XmlElement" /> which
        /// references the given <paramref name="id" />
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public XmlElement GetReferenceIdElement(string id)
        {
            return (from idNodeName in _allowedIdNodeNames
                    select FindIdElements(id, idNodeName)
                    into matchingNodes
                    where !MatchingNodesIsNotPopulated(matchingNodes)
                    select matchingNodes.Single()).FirstOrDefault();
        }

        private List<XmlElement> FindIdElements(string idValue, string idNodeName)
        {
            string xpath = $"//*[@*[local-name()='{idNodeName}' and "
                           + $"namespace-uri()='{Constants.Namespaces.WssSecurityUtility}' and .='{idValue}']]";

            return _document.SelectNodes(xpath).Cast<XmlElement>().ToList();
        }

        private static bool MatchingNodesIsNotPopulated(IReadOnlyCollection<XmlElement> matchingNodes)
        {
            if (matchingNodes.Count <= 0)
            {
                return true;
            }

            if (matchingNodes.Count >= 2)
            {
                throw new CryptographicException("Malformed reference element.");
            }

            return false;
        }

        /// <summary>
        /// Get the <see cref="XmlElement" /> which
        /// contains the Signature
        /// </summary>
        /// <returns></returns>
        public XmlElement GetSignatureElement()
        {
            XmlNode nodeSignature = _document.SelectSingleNode("//*[local-name()='Signature'] ");
            var xmlSignature = nodeSignature as XmlElement;

            if (nodeSignature == null || xmlSignature == null)
            {
                throw new CryptographicException("Invalid Signature: Signature Tag not found");
            }

            return xmlSignature;
        }
    }
}