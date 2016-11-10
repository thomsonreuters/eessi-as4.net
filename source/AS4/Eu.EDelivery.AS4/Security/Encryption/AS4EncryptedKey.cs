using System.Security.Cryptography.Xml;
using System.Xml;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// AS4 Implementation to wrap the <see cref="EncryptedKey"/>
    /// </summary>
    public class AS4EncryptedKey
    {
        private EncryptedKey _encryptedKey;

        /// <summary>
        /// Set the internal <see cref="EncryptedKey"/>
        /// </summary>
        /// <param name="encryptedKey"></param>
        public void SetEncryptedKey(EncryptedKey encryptedKey)
        {
            this._encryptedKey = encryptedKey;
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
            string xpath = $"//*[local-name()='DigestMethod' and namespace-uri()='{Constants.Namespaces.XmlDsig}']";
            var digestNode = this._encryptedKey.GetXml().SelectSingleNode(xpath) as XmlElement;

            return digestNode?.GetAttribute("Algorithm");
        }

        /// <summary>
        /// Load a the <EncryptedKey/> Element inside the <see cref="EncryptedKey"/> Model
        /// </summary>
        /// <param name="xmlDocument"></param>
        public void LoadEncryptedKey(XmlDocument xmlDocument)
        {
            this._encryptedKey = new EncryptedKey();

            var encryptedKeyElement = xmlDocument.SelectSingleNode("//*[local-name()='EncryptedKey']") as XmlElement;
            this._encryptedKey.LoadXml(encryptedKeyElement);
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

        private void AppendDigestMethod(XmlNode encryptionMethodNode)
        {
            XmlElement digestMethod = encryptionMethodNode.OwnerDocument
                .CreateElement("DigestMethod", Constants.Namespaces.XmlDsig);

            // TODO: do we need to change this algorithm (configured by the PMode)
            digestMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#sha256");

            encryptionMethodNode.AppendChild(digestMethod);
        }
    }
}
