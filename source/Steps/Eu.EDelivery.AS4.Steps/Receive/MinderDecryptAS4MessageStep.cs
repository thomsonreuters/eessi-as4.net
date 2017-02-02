using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class MinderDecryptAS4MessageStep : DecryptAS4MessageStep
    {
        protected override Tuple<X509FindType, string> GetCertificateFindValue(InternalMessage internalMessage)
        {
            Decryption decryption = internalMessage.AS4Message.ReceivingPMode.Security.Decryption;

            MessageProperty property = internalMessage.AS4Message.PrimaryUserMessage?.MessageProperties.FirstOrDefault(m => m.Name.Equals("originalSender", StringComparison.OrdinalIgnoreCase));

            string findValue = property?.Value.Equals("C1", StringComparison.OrdinalIgnoreCase) == true ? "as4-net-c3" : "as4-net-c2";

            return new Tuple<X509FindType, string>(decryption.PrivateKeyFindType, findValue);
        }
    }
}
