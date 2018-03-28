using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Eu.EDelivery.AS4.Security.Strategies;

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
        [DefaultValue(Constants.Namespaces.XmlEnc11Aes128)]
        public string EncryptAlgorithm { get; set; }

        [DefaultValue(128)]
        public int EncryptAlgorithmKeySize { get; set; }

        public byte[] EncryptPublicKeyCertificate { get; set; }
        
        public string EncryptPublicKeyCertificateName { get; set; }

        [MaxLength(256)]
        [DefaultValue(EncryptionStrategy.XmlEncSHA256Url)]
        public string EncryptKeyDigestAlgorithm { get; set; }

        [MaxLength(256)]
        public string EncryptKeyMgfAlorithm { get; set; }

        [MaxLength(256)]
        [DefaultValue(EncryptionStrategy.XmlEncRSAOAEPUrlWithMgf)]
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
