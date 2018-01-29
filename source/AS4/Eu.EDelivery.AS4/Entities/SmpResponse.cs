using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eu.EDelivery.AS4.Entities
{
    public class SmpResponse
    {
        [Key]
        [MaxLength(255)]
        public string ToPartyId { get; set; }

        [Key]
        [MaxLength(255)]
        public string PartyRole { get; set; }

        [Key]
        [MaxLength(255)]
        public string PartyType { get; set; }

        [Column("URL")]
        public string Url { get; set; }

        [MaxLength(255)]
        public string ServiceValue { get; set; }

        [MaxLength(255)]
        public string ServiceType { get; set; }

        [MaxLength(255)]
        public string Action { get; set; }

        [Column("TLSEnabled")]
        public bool TlsEnabled { get; set; }

        public bool EncryptionEnabled { get; set; }

        public string FinalRecipient { get; set; }

        public string EncryptAlgorithm { get; set; }

        public int EncryptAlgorithmKeySize { get; set; }

        public string EncryptPublicKeyCertificate { get; set; }

        public string EncryptKeyDigestAlgorithm { get; set; }

        public string EncryptKeyMgfAlorithm { get; set; }

        public string EncryptKeyTransportAlgorithm { get; set; }
    }
}
