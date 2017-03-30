using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Stub Certificate for the Certificate Related Tests
    /// </summary>
    public class StubCertificateRepository : ICertificateRepository, IDisposable
    {
        private readonly X509Store _certificateStore;
        private readonly X509Certificate2 _dummyCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubCertificateRepository"/> class. 
        /// </summary>
        public StubCertificateRepository()
        {
            _dummyCertificate = new X509Certificate2(
                Properties.Resources.holodeck_partya_certificate,
                Properties.Resources.certificate_password,
                X509KeyStorageFlags.Exportable);

            _certificateStore = new X509Store();
            _certificateStore.Open(OpenFlags.ReadWrite);
            _certificateStore.Add(_dummyCertificate);
        }

        public X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference)
        {
            return _certificateStore.Certificates.Find(findType, privateKeyReference, false)[0];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public X509Certificate2 GetDummyCertificate()
        {
            return _dummyCertificate;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _dummyCertificate.Dispose();
        }
    }
}