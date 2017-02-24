using System;
using System.Security.Cryptography.Xml;
using System.Xml;


namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// AS4 Implementation to wrap the <see cref="EncryptedKey"/>
    /// </summary>
    public class AS4EncryptedKey
    {
        private readonly EncryptedKey _encryptedKey;
        private readonly string _digestAlgorithm;
        private readonly string _mgfAlgorithm;

        internal AS4EncryptedKey(EncryptedKey encryptedKey, string digestAlgorithm, string mgfAlgorithm)
        {
            _encryptedKey = encryptedKey;
            _digestAlgorithm = digestAlgorithm;
            _mgfAlgorithm = mgfAlgorithm;
        }

        internal static AS4EncryptedKey LoadFromXmlDocument(XmlDocument xmlDocument)
        {
            var encryptedKeyElement = xmlDocument.SelectSingleNode("//*[local-name()='EncryptedKey']") as XmlElement;

            if (encryptedKeyElement == null)
            {
                throw new ArgumentException(@"No EncryptedKey element found in XmlDocument", nameof(xmlDocument));
            }

            var encryptedKey = new EncryptedKey();

            encryptedKey.LoadXml(encryptedKeyElement);

            return new AS4EncryptedKey(encryptedKey, GetDigestAlgorithm(encryptedKey), GetMgfAlgorithm(encryptedKey));
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

        private static string GetDigestAlgorithm(EncryptedKey encryptedKey)
        {
            string xpath = $".//*[local-name()='EncryptionMethod']/*[local-name()='DigestMethod' and namespace-uri()='{Constants.Namespaces.XmlDsig}']";
            var digestNode = encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return digestNode?.GetAttribute("Algorithm");
        }

        private static string GetMgfAlgorithm(EncryptedKey encryptedKey)
        {
            string xpath = $".//*[local-name()='EncryptionMethod']";

            var node = encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return node?.GetAttribute("MGF");
        }

        /// <summary>
        /// Get the DigestMethod Element from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetDigestAlgorithm()
        {
            return _digestAlgorithm;           
        }

        /// <summary>
        /// Get the MGF (Mask Generation Function) from the <see cref="EncryptedKey"/>
        /// </summary>
        /// <returns></returns>
        public string GetMaskGenerationFunction()
        {            
            return _mgfAlgorithm;
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

            var encryptionMethodNode = encryptedKeyElement.SelectSingleNode("//*[local-name()='EncryptionMethod']");

            if (encryptionMethodNode != null)
            {
                AppendDigestMethod(encryptionMethodNode, _digestAlgorithm);

                if (_mgfAlgorithm != null)
                {

                    AppendMgfMethod(encryptionMethodNode, _mgfAlgorithm);
                }
            }

            return encryptedKeyElement;
        }

        private static void AppendDigestMethod(XmlNode encryptionMethodNode, string digestAlgorithm)
        {
            XmlElement digestMethod = encryptionMethodNode.OwnerDocument
                .CreateElement("DigestMethod", Constants.Namespaces.XmlDsig);
            
            digestMethod.SetAttribute("Algorithm", digestAlgorithm); 

            encryptionMethodNode.AppendChild(digestMethod);
        }

        private static void AppendMgfMethod(XmlNode node, string mgfAlgorithm)
        {
            var mgfAttribute = node.OwnerDocument.CreateAttribute("MGF", mgfAlgorithm);

            node.Attributes.Append(mgfAttribute);
        }
    }
}
