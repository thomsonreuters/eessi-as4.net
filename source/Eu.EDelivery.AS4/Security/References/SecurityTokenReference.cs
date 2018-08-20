using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Eu.EDelivery.AS4.Security.References
{
    /// <summary>
    /// Security Token Reference Base Class to have a consistent AS4 Key Info Clause.
    /// Acts as  Interface for the different Security Token Reference options needed.
    /// </summary>
    public abstract class SecurityTokenReference : KeyInfoClause
    {
        private X509Certificate2 _certificate;

        /// <summary>
        /// Gets or sets the referenced <see cref="X509Certificate2"/>.
        /// </summary>
        public X509Certificate2 Certificate
        {
            get
            {
                if (_certificate == null)
                {
                    _certificate = LoadCertificate();
                }
                return _certificate;
            }
            protected set
            {
                _certificate = value;
            }
        }

        public virtual XmlElement AppendSecurityTokenTo(XmlElement element, XmlDocument document)
        {
            return element;
        }

        protected abstract X509Certificate2 LoadCertificate();

        public abstract override XmlElement GetXml();

        public abstract override void LoadXml(XmlElement element);

        
    }
}