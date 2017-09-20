using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace Eu.EDelivery.AS4.Model.PMode
{
    public class CertificateFindCriteria
    {
        [Description("Find certificate using")]
        public X509FindType CertificateFindType { get; set; }

        [Description("Key value to search for")]
        public string CertificateFindValue { get; set; }
    }
}