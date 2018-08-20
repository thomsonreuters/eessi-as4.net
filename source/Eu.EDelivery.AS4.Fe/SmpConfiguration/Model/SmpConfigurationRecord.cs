namespace Eu.EDelivery.AS4.Fe.SmpConfiguration.Model
{
    public class SmpConfigurationRecord
    {
        public long Id { get; set; }

        public string ToPartyId { get; set; }

        public string PartyType { get; set; }

        public string PartyRole { get; set; }

        public string Url { get; set; }

        public string ServiceValue { get; set; }

        public string ServiceType { get; set; }

        public string Action { get; set; }

        public string FinalRecipient { get; set; }

        public bool TlsEnabled { get; set; }

        public bool EncryptionEnabled { get; set; }
    }
}
