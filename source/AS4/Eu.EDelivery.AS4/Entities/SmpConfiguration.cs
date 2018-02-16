using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    public class SmpConfiguration
    {
        public long Id { get; set; }

        [MaxLength(256)]
        public string ToPartyId { get; set; }

        [MaxLength(256)]
        public string PartyRole { get; set; }

        [MaxLength(256)]
        public string PartyType { get; set; }

        [Column("URL")]
        [MaxLength(2083)]
        public string Url { get; set; }

        [MaxLength(256)]
        public string ServiceValue { get; set; }

        [MaxLength(256)]
        public string ServiceType { get; set; }

        [MaxLength(256)]
        public string Action { get; set; }

        [Column("TLSEnabled")]
        public bool TlsEnabled { get; set; }

        public bool EncryptionEnabled { get; set; }

        [MaxLength(256)]
        public string FinalRecipient { get; set; }

        [MaxLength(256)]
        public string EncryptAlgorithm { get; set; }

        public int EncryptAlgorithmKeySize { get; set; }

        [MaxLength(256)]
        public string EncryptPublicKeyCertificate { get; set; }

        [MaxLength(256)]
        public string EncryptKeyDigestAlgorithm { get; set; }

        [MaxLength(256)]
        public string EncryptKeyMgfAlorithm { get; set; }

        [MaxLength(256)]
        public string EncryptKeyTransportAlgorithm { get; set; }
    }
}
