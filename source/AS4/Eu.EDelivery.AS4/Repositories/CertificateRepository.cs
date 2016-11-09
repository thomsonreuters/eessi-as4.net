using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Common;

namespace Eu.EDelivery.AS4.Repositories
{
    /// <summary>
    /// Repository to expose the Certificate from the Certificate Store
    /// </summary>
    public class CertificateRepository : ICertificateRepository
    {
        private readonly IConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateRepository"/> class
        /// with default Configuration
        /// </summary>
        public CertificateRepository()
        {
            this._config = Config.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateRepository"/> class
        /// Create a Certificate Repository with a given Configuration
        /// </summary>
        /// <param name="config">
        /// </param>
        public CertificateRepository(IConfig config)
        {
            this._config = config;
        }

        /// <summary>
        /// Get the <see cref="X509Certificate2"/>
        /// from the Certificate Store
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="privateKeyReference"></param>
        /// <returns></returns>
        public X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference)
        {
            X509Store certificateStore = GetCertificateStore();
            certificateStore.Open(OpenFlags.ReadOnly);

            return certificateStore.Certificates
                .Find(findType, privateKeyReference, validOnly: false)[0];
        }

        private X509Store GetCertificateStore()
        {
            return new X509Store(
                this._config.GetSetting("certificatestore"), StoreLocation.LocalMachine);
        }
    }

    public interface ICertificateRepository
    {
        X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference);
    }
}