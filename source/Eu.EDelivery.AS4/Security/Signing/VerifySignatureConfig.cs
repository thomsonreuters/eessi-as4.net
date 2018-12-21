using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;

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
        public VerifySignatureConfig(
            bool allowUnknownRootCertificateAuthority, 
            IEnumerable<Attachment> attachments)
            : this(
                allowUnknownRootCertificateAuthority, 
                attachments, 
                Registry.Instance.CertificateRepository) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerifySignatureConfig"/> class.
        /// </summary>
        /// <param name="allowUnknownRootCertificateAuthority">if set to <c>true</c> [allow unknown root certificate authority].</param>
        /// <param name="attachments">The attachments.</param>
        /// <param name="certificateRepository">The certificate repository used to retrieve the signing certificate that was referenced in the message.</param>
        public VerifySignatureConfig(
            bool allowUnknownRootCertificateAuthority, 
            IEnumerable<Attachment> attachments, 
            ICertificateRepository certificateRepository)
        {
            AllowUnknownRootCertificateAuthority = allowUnknownRootCertificateAuthority;
            Attachments = attachments ?? Enumerable.Empty<Attachment>();
            CertificateRepository = certificateRepository ?? Registry.Instance.CertificateRepository;
        }

        public bool AllowUnknownRootCertificateAuthority { get; }

        public IEnumerable<Attachment> Attachments { get; }

        public ICertificateRepository CertificateRepository { get; }
    }
}
