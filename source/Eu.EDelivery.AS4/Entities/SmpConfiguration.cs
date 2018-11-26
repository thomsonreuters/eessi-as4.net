using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eu.EDelivery.AS4.Security.Strategies;

namespace Eu.EDelivery.AS4.Entities
{
    public class SmpConfiguration
    {
        public long Id { get; set; }

        [Required]
        [MaxLength(256)]
        [Info("To party id")]
        public string ToPartyId { get; set; }

        [Required]
        [MaxLength(256)]
        [Info("Part role")]
        public string PartyRole { get; set; }

        [Required]
        [MaxLength(256)]
        [Info("Party type")]
        public string PartyType { get; set; }

        [Column("URL")]
        [MaxLength(2083)]
        [Info("Url")]
        public string Url { get; set; }

        [MaxLength(256)]
        [Info("Service value")]
        public string ServiceValue { get; set; }

        [MaxLength(256)]
        [Info("Service type")]
        public string ServiceType { get; set; }

        [MaxLength(256)]
        [Info("Action")]
        public string Action { get; set; }

        [Column("TLSEnabled")]
        [Info("TLS enabled")]
        public bool TlsEnabled { get; set; }

        [Info("Encryption enabled")]
        public bool EncryptionEnabled { get; set; }

        [MaxLength(256)]
        [Info("Final recipient")]
        public string FinalRecipient { get; set; }

        [MaxLength(256)]
        [DefaultValue(Constants.Namespaces.XmlEnc11Aes128)]
        [Info("Encryption algorithm")]
        public string EncryptAlgorithm { get; set; }

        [DefaultValue(128)]
        [Info("Encryption algorithm size")]
        public int EncryptAlgorithmKeySize { get; set; }

        [Info("Encryption public key certificate")]
        public byte[] EncryptPublicKeyCertificate { get; set; }
        
        [Info("Encryption public key certificate name")]
        public string EncryptPublicKeyCertificateName { get; set; }

        [MaxLength(256)]
        [DefaultValue(EncryptionStrategy.XmlEncSHA256Url)]
        [Info("Encryption key digest algorithm")]
        public string EncryptKeyDigestAlgorithm { get; set; }

        [MaxLength(256)]
        [Info("Encryption key mgf algorithm")]
        public string EncryptKeyMgfAlorithm { get; set; }

        [MaxLength(256)]
        [DefaultValue(EncryptionStrategy.XmlEncRSAOAEPUrlWithMgf)]
        [Info("Encryption key transport algorithm")]
        public string EncryptKeyTransportAlgorithm { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmpConfiguration"/> class.
        /// </summary>
        public SmpConfiguration()
        {
            EncryptAlgorithmKeySize = 128;
            EncryptAlgorithm = Constants.Namespaces.XmlEnc11Aes128;
            EncryptKeyDigestAlgorithm = EncryptionStrategy.XmlEncSHA256Url;
            EncryptKeyTransportAlgorithm = EncryptionStrategy.XmlEncRSAOAEPUrlWithMgf;
        }
    }
}
