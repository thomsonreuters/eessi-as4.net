using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using Eu.EDelivery.AS4.Security.References;

namespace Eu.EDelivery.AS4.Security.Repositories
{
    /// <summary>
    /// Repository to navigate the <see cref="KeyInfo"/> Model
    /// </summary>
    internal class KeyInfoRepository
    {
        private readonly KeyInfo _keyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyInfoRepository"/> class
        /// </summary>
        /// <param name="keyInfo"></param>
        public KeyInfoRepository(KeyInfo keyInfo)
        {
            this._keyInfo = keyInfo;
        }

        public X509Certificate2 GetCertificate()
        {
            if (this._keyInfo == null) return null;
            foreach (object keyInfo in this._keyInfo)
            {
                // Embedded (is this actually allowed?)
                var embeddedCertificate = keyInfo as KeyInfoX509Data;
                if (embeddedCertificate != null && embeddedCertificate.Certificates.Count > 0)
                    return embeddedCertificate.Certificates[0] as X509Certificate2;

                // Reference
                var securityTokenReference = keyInfo as SecurityTokenReference;
                if (securityTokenReference != null)
                    return securityTokenReference.Certificate;
            }
            return null;
        }
    }
}
