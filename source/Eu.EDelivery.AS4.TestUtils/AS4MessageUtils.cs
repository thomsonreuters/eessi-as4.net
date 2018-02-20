using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Encryption;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.TestUtils
{
    public static class AS4MessageUtils
    {
        public static AS4Message SignWithCertificate(AS4Message message, X509Certificate2 certificate)
        {
            var config = new CalculateSignatureConfig(certificate,
                X509ReferenceType.BSTReference,
                Constants.SignAlgorithms.Sha256,
                Constants.HashFunctions.Sha256);

            var signer = SignStrategy.ForAS4Message(message, config);

            message.SecurityHeader.Sign(signer);

            return message;
        }

        public static AS4Message EncryptWithCertificate(AS4Message message, X509Certificate2 certificate)
        {
            var encryption = EncryptionStrategyBuilder.Create(message, new KeyEncryptionConfiguration(certificate))
                                                      .Build();

            message.SecurityHeader.Encrypt(encryption);

            return message;
        }

        public static async Task<AS4Message> SerializeDeserializeAsync(AS4Message message)
        {
            var serializer = SerializerProvider.Default.Get(message.ContentType);

            using (var targetStream = new MemoryStream())
            {
                serializer.Serialize(message, targetStream, CancellationToken.None);

                targetStream.Position = 0;

                return await serializer.DeserializeAsync(targetStream, message.ContentType, CancellationToken.None);
            }
        }
    }
}
