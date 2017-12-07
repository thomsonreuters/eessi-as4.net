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
        public bool AllowUnknownRootCertificateAuthority { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
    }
}
