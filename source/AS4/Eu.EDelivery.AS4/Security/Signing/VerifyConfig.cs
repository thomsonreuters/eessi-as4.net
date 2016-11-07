using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Security.Signing
{
    /// <summary>
    /// Configuration Options for
    /// the verification of the <see cref="AS4Message"/>
    /// </summary>
    public class VerifyConfig
    {
        public ICertificateRepository CertificateRepository { get; set; }
        public ICollection<Attachment> Attachments { get; set; }
    }
}
