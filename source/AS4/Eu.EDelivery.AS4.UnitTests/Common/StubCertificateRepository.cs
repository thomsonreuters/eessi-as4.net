using System;
using System.Security.Cryptography.X509Certificates;
using Eu.EDelivery.AS4.Repositories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Stub Certificate for the Certificate Related Tests
    /// </summary>
    public sealed class StubCertificateRepository : ICertificateRepository, IDisposable
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

        /// <summary>
        /// Find a <see cref="X509Certificate2"/> based on the given <paramref name="privateKeyReference"/> for the <paramref name="findType"/>.
        /// </summary>
        /// <param name="findType">Kind of searching approach.</param>
        /// <param name="privateKeyReference">Value to search in the repository.</param>
        /// <returns></returns>
        public X509Certificate2 GetCertificate(X509FindType findType, string privateKeyReference)
        {
            return _certificateStore.Certificates.Find(findType, privateKeyReference, validOnly: false)[0];
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