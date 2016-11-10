using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;
using Castle.Core.Internal;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Security.Serializers
{
    /// <summary>
    /// Serializer to select <see cref="EncryptedData"/> Models
    /// </summary>
    internal class EncryptedDataSerializer
    {
        private readonly XmlDocument _document;
        private readonly IList<EncryptedData> _encryptedDatas;
        private const string EncNamespace = "http://www.w3.org/2001/04/xmlenc#";

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedDataSerializer"/> class
        /// </summary>
        /// <param name="document"></param>
        public EncryptedDataSerializer(XmlDocument document)
        {
            this._document = document;
            this._encryptedDatas = new List<EncryptedData>();
        }

        /// <summary>
        /// Serialize the configured Xml Document to <see cref="EncryptedData"/> Models
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EncryptedData> SerializeEncryptedDatas()
        {
            SerializeEncryptedDataElements();
            return this._encryptedDatas;
        }

        private void SerializeEncryptedDataElements()
        {
            var namespaceManager = new XmlNamespaceManager(this._document.NameTable);
            namespaceManager.AddNamespace("enc", EncNamespace);

            IEnumerable<XmlElement> encryptedDataElements = this._document
                .SelectNodes("//enc:EncryptedData", namespaceManager)
                ?.OfType<XmlElement>();
                
            encryptedDataElements.ForEach(SerializeEncryptedData);

            if (encryptedDataElements == null)
                throw new AS4Exception("No EncryptedData elements found to decrypt");
        }

        private void SerializeEncryptedData(XmlElement e)
        {
            var encryptedData = new EncryptedData();
            encryptedData.LoadXml(e);
            this._encryptedDatas.Add(encryptedData);
        }
    }
}
