using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.Algorithms;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Signing;

namespace Eu.EDelivery.AS4.Security.Strategies
{
    public interface ISigningStrategy
    {
        ArrayList References { get; }
        SecurityTokenReference SecurityTokenReference { get; }

        void AddAlgorithm(SignatureAlgorithm algorithm);
        void AddAttachmentReference(Attachment attachment, string digestMethod);
        void AddCertificate(X509Certificate2 certificate);
        void AddXmlReference(string id, string hashFunction);

        void AppendSignature(XmlElement securityElement);
        bool VerifySignature(VerifyConfig options);
        void SignSignature();
    }
}