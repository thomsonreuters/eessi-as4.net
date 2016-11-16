using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Repositories;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Dummy Certificate for the Certificate Related Tests
    /// </summary>
    public class StubCertificateRepository : ICertificateRepository, IDisposable
    {
        private readonly X509Certificate2 _dummyCertificate;
        private readonly X509Store _certificateStore;

        public StubCertificateRepository()
        {
            this._dummyCertificate = new X509Certificate2(
                  rawData: Properties.Resources.holodeck_partya_certificate,
                  password: Properties.Resources.certificate_password,
                  keyStorageFlags: X509KeyStorageFlags.Exportable);

            this._certificateStore = new X509Store();
            this._certificateStore.Open(OpenFlags.ReadWrite);
            this._certificateStore.Add(this._dummyCertificate);
        }

        public X509Certificate2 GetDummyCertificate()
        {
            return this._dummyCertificate;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this._dummyCertificate.Dispose();
        }

        public X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference)
        {
            return this._certificateStore.Certificates
                .Find(findType, privateKeyReference, false)[0];
        }
    }
}
