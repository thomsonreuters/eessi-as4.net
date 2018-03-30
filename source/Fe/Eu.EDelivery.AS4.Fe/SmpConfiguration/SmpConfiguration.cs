namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     Smp configuration model
    /// </summary>
    public class SmpConfiguration
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        public long Id { get; set; }

        /// <summary>
        ///     Gets or sets to party identifier.
        /// </summary>
        /// <value>
        ///     To party identifier.
        /// </value>
        public string ToPartyId { get; set; }
        
        /// <summary>
        /// Gets or sets the party role.
        /// </summary>
        /// <value>
        /// The party role.
        /// </value>
        public string PartyRole { get; set; }

        /// <summary>
        ///     Gets or sets the type of the party.
        /// </summary>
        /// <value>
        ///     The type of the party.
        /// </value>
        public string PartyType { get; set; }

        /// <summary>
        ///     Gets or sets the URL.
        /// </summary>
        /// <value>
        ///     The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        ///     Gets or sets the service value.
        /// </summary>
        /// <value>
        ///     The service value.
        /// </value>
        public string ServiceValue { get; set; }

        /// <summary>
        ///     Gets or sets the type of the service.
        /// </summary>
        /// <value>
        ///     The type of the service.
        /// </value>
        public string ServiceType { get; set; }

        /// <summary>
        ///     Gets or sets the action.
        /// </summary>
        /// <value>
        ///     The action.
        /// </value>
        public string Action { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [TLS enabled].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [TLS enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool TlsEnabled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [encryption enabled].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [encryption enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool EncryptionEnabled { get; set; }

        /// <summary>
        ///     Gets or sets the final recipient.
        /// </summary>
        /// <value>
        ///     The final recipient.
        /// </value>
        public string FinalRecipient { get; set; }

        /// <summary>
        ///     Gets or sets the encrypt algorithm.
        /// </summary>
        /// <value>
        ///     The encrypt algorithm.
        /// </value>
        public string EncryptAlgorithm { get; set; }

        /// <summary>
        ///     Gets or sets the size of the encrypt algorithm key.
        /// </summary>
        /// <value>
        ///     The size of the encrypt algorithm key.
        /// </value>
        public int EncryptAlgorithmKeySize { get; set; }

        /// <summary>
        ///     Gets or sets the encrypt public key certificate.
        /// </summary>
        /// <value>
        ///     The encrypt public key certificate.
        /// </value>
        public string EncryptPublicKeyCertificate { get; set; }

        /// <summary>
        ///     Gets or sets the name of the encrypt public key certificate.
        /// </summary>
        /// <value>
        ///     The name of the encrypt public key certificate.
        /// </value>
        public string EncryptPublicKeyCertificateName { get; set; }

        /// <summary>
        ///     Gets or sets the encrypt key digest algorithm.
        /// </summary>
        /// <value>
        ///     The encrypt key digest algorithm.
        /// </value>
        public string EncryptKeyDigestAlgorithm { get; set; }

        /// <summary>
        ///     Gets or sets the encrypt key MGF alorithm.
        /// </summary>
        /// <value>
        ///     The encrypt key MGF alorithm.
        /// </value>
        public string EncryptKeyMgfAlorithm { get; set; }

        /// <summary>
        ///     Gets or sets the encrypt key transport algorithm.
        /// </summary>
        /// <value>
        ///     The encrypt key transport algorithm.
        /// </value>
        public string EncryptKeyTransportAlgorithm { get; set; }
    }
}