using Eu.EDelivery.AS4.Security.Transforms;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    public class DataEncryptionConfiguration
    {
        public string EncryptionMethod { get; private set; }

        public string EncryptionType { get; private set; }

        public string TransformAlgorithm { get; private set; }

        public DataEncryptionConfiguration(string encryptionMethod, string encryptionType = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Only", string transformAlgorithm = AttachmentCiphertextTransform.Url)
        {
            EncryptionMethod = encryptionMethod;
            EncryptionType = encryptionType;
            TransformAlgorithm = transformAlgorithm;
        }
    }
}