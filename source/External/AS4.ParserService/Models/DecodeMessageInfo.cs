using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AS4.ParserService.Models
{
    public class DecodeMessageInfo
    {
        /// <summary>
        /// A byte array that represents the Message that must be decoded.
        /// </summary>
        public byte[] ReceivedMessage { get; set; }

        /// <summary>
        /// The ContentType of the received message
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The Receiving PMode that must be used to decode the Received Message.
        /// </summary>
        public byte[] ReceivingPMode { get; set; }

        /// <summary>
        /// The certificate that must be used to decrypt the received message.
        /// </summary>
        public byte[] DecryptionCertificate { get; set; }

        /// <summary>
        /// The password to access the decryption certificate.
        /// </summary>
        public string DecryptionCertificatePassword { get; set; }

        /// <summary>
        /// The Sending PMode that must be used to create the responding SignalMessage.
        /// </summary>
        public byte[] RespondingPMode { get; set; }

        public byte[] SigningResponseCertificate { get; set; }

        public string SigningResponseCertificatePassword { get; set; }
    }
}