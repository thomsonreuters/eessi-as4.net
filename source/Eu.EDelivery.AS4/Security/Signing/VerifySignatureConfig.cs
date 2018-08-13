using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Security.Signing
{
    /// <summary>
    /// Configuration Options for
    /// the verification of the <see cref="AS4Message"/>
    /// </summary>
    public class VerifySignatureConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifySignatureConfig"/> class.
        /// </summary>
        /// <param name="allowUnknownRootCertificateAuthority">if set to <c>true</c> [allow unknown root certificate authority].</param>
        /// <param name="attachments">The attachments.</param>
        public VerifySignatureConfig(bool allowUnknownRootCertificateAuthority, IEnumerable<Attachment> attachments)
        {
            AllowUnknownRootCertificateAuthority = allowUnknownRootCertificateAuthority;
            Attachments = attachments;
        }

        public bool AllowUnknownRootCertificateAuthority { get; }

        public IEnumerable<Attachment> Attachments { get; }
    }
}
