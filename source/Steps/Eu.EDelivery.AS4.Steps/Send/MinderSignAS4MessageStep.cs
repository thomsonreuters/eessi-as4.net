using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Steps.Send
{
    public class MinderSignAS4MessageStep : SignAS4MessageStep
    {
        protected override Tuple<X509FindType, string> GetCertificateFindValue(AS4Message as4Message)
        {
            var signing = as4Message.SendingPMode.Security.Signing;

            MessageProperty property = as4Message.PrimaryUserMessage?.MessageProperties.FirstOrDefault(m => m.Name.Equals("originalSender", StringComparison.OrdinalIgnoreCase));
            string findValue = property?.Value.Equals("C1", StringComparison.OrdinalIgnoreCase) == true ? "as4-net-c2" : "as4-net-c3";

            return new Tuple<X509FindType, string>(signing.PrivateKeyFindType, findValue);
        }
    }
}