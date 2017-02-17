using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Steps.Send
{
    [Obsolete("This Step should not be used anymore")]
    public class MinderEncryptAS4MessageStep : EncryptAS4MessageStep
    {
        protected override Tuple<X509FindType, string> GetCertificateFindValue(AS4Message as4Message)
        {
            var encryption = as4Message.SendingPMode.Security.Encryption;

            MessageProperty property = as4Message.PrimaryUserMessage?.MessageProperties.FirstOrDefault(m => m.Name.Equals("originalSender", StringComparison.OrdinalIgnoreCase));
            string findValue = property?.Value.Equals("C1") == true ? "as4-net-c3" : "as4-net-c2";

            return new Tuple<X509FindType, string>(encryption.PublicKeyFindType, findValue);
        }
    }
}