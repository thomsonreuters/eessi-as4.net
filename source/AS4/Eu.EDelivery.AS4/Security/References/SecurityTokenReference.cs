using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

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
        /// Gets or sets the reference id.
        /// </summary>
        public string ReferenceId { get; protected set; } = "cert-" + Guid.NewGuid();

        public virtual XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            return element;
        }

        public abstract override XmlElement GetXml();

        public abstract override void LoadXml(XmlElement element);

        
    }
}