using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Builders.Security;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;

namespace Eu.EDelivery.AS4.TestUtils
{
    public static class AS4MessageUtils
    {
        public static AS4Message SignWithCertificate(AS4Message message, X509Certificate2 certificate)
        {
            var signing = new SigningStrategyBuilder(message, X509ReferenceType.BSTReference)
               .WithCertificate(certificate)
               .WithSigningId(message.SigningId, hashFunction: Constants.HashFunctions.First())
               .WithSignatureAlgorithm(Constants.Algoritms.First())
               .Build();

            message.SecurityHeader.Sign(signing);

            return message;
        }

        public static AS4Message EncryptWithCertificate(AS4Message message, X509Certificate2 certificate)
        {
            var encryption = EncryptionStrategyBuilder.Create(message)
                                                      .WithCertificate(certificate)
                                                      .WithAttachments(message.Attachments)
                                                      .Build();

            message.SecurityHeader.Encrypt(encryption);

            return message;
        }
    }
}
