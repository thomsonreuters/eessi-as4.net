using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Security.References;

namespace Eu.EDelivery.AS4.Security.Encryption
{
    /// <summary>
    /// Wrapper for Encryption Specific Configuration
    /// </summary>
    [Obsolete]
    internal class EncryptionConfiguration
    {
        private X509Certificate2 _certificate;
        public DataEncryptionConfiguration Data { get; set; }// = new DataEncryptionConfiguration();
        public KeyEncryptionConfiguration Key { get; set; } // = new KeyEncryptionConfiguration(new BinarySecurityTokenReference());
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