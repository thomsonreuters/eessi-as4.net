using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Security Token Reference Base Class to have a consistent AS4 Key Info Clause.
    /// Acts as  Interface for the different Security Token Reference options needed.
    /// TODO: responsible for concrete implementation based on XmlDocument and Reference Type (Enum)
    /// </summary>
    public abstract class SecurityTokenReference : KeyInfoClause
    {
        /// <summary>
        /// Gets or sets the referenced <see cref="X509Certificate2"/>.
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ICertificateRepository"/> the retrieve the <see cref="X509Certificate2"/>
        /// </summary>
        public ICertificateRepository CertificateRepository { protected get; set; }

        /// <summary>
        /// Gets or sets the reference id.
        /// </summary>
        public string ReferenceId { get; set; } = "cert-" + Guid.NewGuid();

        public virtual XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            return element;
        }

        public abstract override XmlElement GetXml();

        public abstract override void LoadXml(XmlElement element);

        /// <summary>
        /// Load the given <see cref="envelopeDocument"/> 
        /// in the <see cref="SecurityTokenReference"/>
        /// </summary>
        /// <param name="envelopeDocument"></param>
        public void LoadXml(XmlDocument envelopeDocument)
        {
            // TODO: we need to make sure that this gets implemented correctly.
            // the correct SecurityTokenReference should be retrieved.
            // Take a look at the implementation in Conformance-testing branch.

            var securityTokenElement =
                envelopeDocument.SelectSingleNode("//*[local-name()='SecurityTokenReference'] ") as XmlElement;

            if (securityTokenElement == null)
                throw AS4ExceptionBuilder
                    .WithDescription("No Security Token Reference element found in given Xml Document")
                    .WithErrorCode(ErrorCode.Ebms0101)
                    .Build();

            this.LoadXml(securityTokenElement);
        }
    }
}