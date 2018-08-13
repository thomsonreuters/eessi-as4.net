using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Stub Certificate for the Certificate Related Tests
    /// </summary>
    [NotConfigurable]
    public sealed class StubCertificateRepository : ICertificateRepository, IDisposable
    {
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

            CertificateStore = new X509Store();
            CertificateStore.Open(OpenFlags.ReadWrite);
            CertificateStore.Add(_dummyCertificate);
        }

        public X509Store CertificateStore { get; }

        /// <summary>
        /// Find a <see cref="X509Certificate2"/> based on the given <paramref name="privateKeyReference"/> for the <paramref name="findType"/>.
        /// </summary>
        /// <param name="findType">Kind of searching approach.</param>
        /// <param name="privateKeyReference">Value to search in the repository.</param>
        /// <returns></returns>
        public X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference)
        {
            return CertificateStore.Certificates.Find(findType, privateKeyReference, validOnly: false)[0];
        }

        /// <summary>
        /// Get a 'Stub' certificate that can be used during signing/encryption.
        /// </summary>
        /// <returns></returns>
        public X509Certificate2 GetStubCertificate()
        {
            return _dummyCertificate;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _dummyCertificate.Dispose();
        }

        [Fact]
        public void IsDisposed()
        {
            // Arrange
            var sut = new StubCertificateRepository();
            sut.Dispose();

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => sut._dummyCertificate.PrivateKey);
        }
    }
}