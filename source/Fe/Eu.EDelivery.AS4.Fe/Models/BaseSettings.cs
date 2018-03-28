using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Fe.Models
{
    public class BaseSettings
    {
        public string IdFormat { get; set; }
        public int RetentionPeriod { get; set; }
        public CertificateStore CertificateStore { get; set; }
    }
}