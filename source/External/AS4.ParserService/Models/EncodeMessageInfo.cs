using System.Collections.Generic;

namespace AS4.ParserService.Models
{
    /// <summary>
    /// Contains all information that is required to create an AS4 Message
    /// </summary>
    public class EncodeMessageInfo
    {
        /// <summary>
        /// The Sending Processing Mode that must be used to create the AS4 Message
        /// </summary>
        public byte[] SendingPMode { get; set; }

        /// <summary>
        /// The Signing Certificate that must to sign the AS4 Message when signing is enabled.
        /// </summary>
        public byte[] SigningCertificate { get; set; }

        /// <summary>
        /// The password of the signing-certificate
        /// </summary>
        public string SigningCertificatePassword { get; set; }

        /// <summary>
        /// The public encryption key that must be used to encrypt the AS4 Message when encryption is enabled.
        /// </summary>
        public byte[] EncryptionPublicKeyCertificate { get; set; }

        /// <summary>
        /// The payloads that must be sent as an attachment in the AS4 Message
        /// </summary>
        public ICollection<PayloadInfo> Payloads { get; set; }
    }
}