using System.Security.Cryptography.X509Certificates;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// Wrapper for Encryption Specific Configuration
    /// </summary>
    internal class EncryptionConfiguration
    {
        private X509Certificate2 _certificate;
        public DataEncryptionConfiguration Data { get; set; } = new DataEncryptionConfiguration();
        public KeyEncryptionConfiguration Key { get; set; } = new KeyEncryptionConfiguration();
        public X509Certificate2 Certificate
        {
            get { return _certificate; }
            set
            {
                this._certificate = value;
                this.Key.SecurityTokenReference.Certificate = value;
            }
        }
    }
}