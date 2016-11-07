using Eu.EDelivery.AS4.Security.Transforms;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    internal class DataEncryptionConfiguration
    {
        public string EncryptionMethod { get; set; } = "http://www.w3.org/2009/xmlenc11#aes128-gcm";

        public string EncryptionType { get; set; } = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Only";

        public string TransformAlgorithm { get; set; } = AttachmentCiphertextTransform.Url;
    }
}