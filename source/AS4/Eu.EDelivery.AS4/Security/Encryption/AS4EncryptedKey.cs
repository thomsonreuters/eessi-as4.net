using System;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// AS4 Implementation to wrap the <see cref="EncryptedKey"/>
    /// </summary>
    public class AS4EncryptedKey
    {
        private readonly EncryptedKey _encryptedKey;

        public AS4EncryptedKey(EncryptedKey encryptedKey)
        {
            _encryptedKey = encryptedKey;
        }

        public static AS4EncryptedKey LoadFromXmlDocument(XmlDocument xmlDocument)
        {            
            var encryptedKeyElement = xmlDocument.SelectSingleNode("//*[local-name()='EncryptedKey']") as XmlElement;

            if (encryptedKeyElement == null)
            {
                throw new ArgumentException(@"No EncryptedKey element found in XmlDocument", nameof(xmlDocument));
            }

            var encryptedKey = new EncryptedKey();

            encryptedKey.LoadXml(encryptedKeyElement);

            return new AS4EncryptedKey(encryptedKey);
        }
        
        /// <summary>
        /// Add a <see cref="DataReference"/> with a given <paramref name="uri"/>
        /// </summary>
        /// <param name="uri"></param>
        public void AddDataReference(string uri)
        {
            this._encryptedKey.ReferenceList.Add(new DataReference(uri));
        }

        /// <summary>
        /// Get the Reference Id from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetReferenceId()
        {
            return this._encryptedKey.Id;
        }

        /// <summary>
        /// Get the <see cref="CipherData"/> from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public CipherData GetCipherData()
        {
            return this._encryptedKey.CipherData;
        }

        /// <summary>
        /// Get the DigestMethod Element from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetDigestAlgorithm()
        {
            string xpath = $".//*[local-name()='EncryptionMethod']/*[local-name()='DigestMethod' and namespace-uri()='{Constants.Namespaces.XmlDsig}']";
            var digestNode = this._encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return digestNode?.GetAttribute("Algorithm");
        }

        /// <summary>
        /// Get the MGF (Mask Generation Function) from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetMaskGenerationFunction()
        {
            string xpath = $".//*[local-name()='EncryptionMethod']";

            var node = _encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return node?.GetAttribute("MGF");
        }
        
        /// <summary>
        /// Append the <EncryptedKey/> Element to a given <paramref name="securityElement"/>
        /// </summary>
        /// <param name="securityElement"></param>
        public void AppendEncryptedKey(XmlElement securityElement)
        {
            XmlElement encryptedKeyElement = GetEncryptedKeyElement();
            XmlNode importedEncryptedKeyNode = securityElement.OwnerDocument.ImportNode(encryptedKeyElement, deep: true);

            securityElement.AppendChild(importedEncryptedKeyNode);
        }

        private XmlElement GetEncryptedKeyElement()
        {
            XmlElement encryptedKeyElement = this._encryptedKey.GetXml();
            AppendDigestMethod(encryptedKeyElement.SelectSingleNode("//*[local-name()='EncryptionMethod']"));

            return encryptedKeyElement;
        }

        private static void AppendDigestMethod(XmlNode encryptionMethodNode)
        {
            XmlElement digestMethod = encryptionMethodNode.OwnerDocument
                .CreateElement("DigestMethod", Constants.Namespaces.XmlDsig);

            // TODO: do we need to change this algorithm (configured by the PMode)
            digestMethod.SetAttribute("Algorithm", EncryptionStrategy.XmlEncSHA1Url);

            encryptionMethodNode.AppendChild(digestMethod);
        }
    }
}
