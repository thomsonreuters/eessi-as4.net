using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Eu.EDelivery.AS4.Security.Serializers
{
    /// <summary>
    /// Serializer to select <see cref="EncryptedData"/> Models
    /// </summary>
    internal class EncryptedDataSerializer
    {
        private readonly XmlDocument _document;
                
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptedDataSerializer"/> class
        /// </summary>
        /// <param name="document"></param>
        public EncryptedDataSerializer(XmlDocument document)
        {
            _document = document;            
        }

        /// <summary>
        /// Serialize the configured Xml Document to <see cref="EncryptedData"/> Models
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EncryptedData> SerializeEncryptedDatas()
        {
            return SerializeEncryptedDataElements();            
        }

        private List<EncryptedData> SerializeEncryptedDataElements()
        {
            var result = new List<EncryptedData>();

            var namespaceManager = new XmlNamespaceManager(_document.NameTable);
            namespaceManager.AddNamespace("enc", Constants.Namespaces.XmlEnc);

            IEnumerable<XmlElement> encryptedDataElements = _document
                .SelectNodes("//enc:EncryptedData", namespaceManager)
                ?.OfType<XmlElement>();

            if (encryptedDataElements == null)
            {
                throw new XmlException("No EncryptedData elements found to decrypt");
            }

            foreach (var e in encryptedDataElements)
            {
                result.Add(SerializeEncryptedData(e));
            }

            return result;
        }

        private static EncryptedData SerializeEncryptedData(XmlElement e)
        {
            var encryptedData = new EncryptedData();
            encryptedData.LoadXml(e);
            return encryptedData;
        }
    }
}
