using System;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.SmpConfiguration;
using Eu.EDelivery.AS4.Fe.SmpConfiguration.Model;
using Eu.EDelivery.AS4.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class SmpConfigurationServiceTest
    {
        private const string Base64CertHeader = "data:application/x-x509-ca-cert;base64,";
        private const string EncryptionCertificateContents =
            "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";
        
        private readonly SmpConfigurationDetail _smpConfiguration;

        public DatastoreContext DbContext { get; private set; }
        public DbContextOptions<DatastoreContext> Options { get; }
        public SmpConfigurationService SmpConfigurationService { get; private set; }
        public MapperConfiguration MapperConfig { get; }

        public SmpConfigurationServiceTest()
        {
            _smpConfiguration = new SmpConfigurationDetail
            {
                Action = "Action",
                EncryptAlgorithm = "EncryptAlgorithm",
                EncryptAlgorithmKeySize = 10,
                EncryptKeyDigestAlgorithm = "EncryptKeyDigestAlgorithm",
                EncryptKeyMgfAlorithm = "EncryptKeyMgfAlorithm",
                EncryptKeyTransportAlgorithm = "EncryptKeyTransportAlgorithm",
                EncryptPublicKeyCertificateName = "AccessPointA.cer",
                EncryptPublicKeyCertificate = Base64CertHeader + EncryptionCertificateContents,
                EncryptionEnabled = true,
                FinalRecipient = "FinalRecipient",
                PartyRole = "PartyRole",
                PartyType = "PartyType",
                ServiceType = "ServiceType",
                ServiceValue = "ServiceValue",
                TlsEnabled = true,
                ToPartyId = "ToPartyId",
                Url = "Url"
            };

            Options = new DbContextOptionsBuilder<DatastoreContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var store = new DatastoreContext(Options, StubConfig.Default))
            {
                store.Database.EnsureCreated();
            }

            CreateNewDbContext();

            MapperConfig = new MapperConfiguration(cfg => { cfg.AddProfile(new SmpConfigurationAutoMapperProfile()); });

            CreateNewSmpConfigurationService();
        }

        protected SmpConfigurationServiceTest CreateNewDbContext()
        {
            DbContext = new DatastoreContext(Options, StubConfig.Default);
            return this;
        }

        protected SmpConfigurationServiceTest CreateNewSmpConfigurationService()
        {
            SmpConfigurationService = new SmpConfigurationService(DbContext, MapperConfig.CreateMapper());
            return this;
        }

        public class Create : SmpConfigurationServiceTest
        {
            [Fact]
            public async Task Creates_NewSmpConfiguration()
            {
                // Arrange
                await SmpConfigurationService.CreateAsync(_smpConfiguration);

                // Act
                var configurationFromDatabase = await DbContext.SmpConfigurations.FirstOrDefaultAsync();

                // Assert
                Assert.NotNull(configurationFromDatabase);

                // Set the Id because this one is generated by the database and makes it possible to compare using JsonConvert
                _smpConfiguration.Id = configurationFromDatabase.Id;
                _smpConfiguration.EncryptPublicKeyCertificate = 
                    _smpConfiguration.EncryptPublicKeyCertificate
                                     .Replace(Base64CertHeader, String.Empty);

                Assert.Equal(
                    JsonConvert.SerializeObject(_smpConfiguration),
                    JsonConvert.SerializeObject(configurationFromDatabase));
            }

            [Fact]
            public async Task ThrowsException_WhenParameterIsNull()
            {
                // Act / Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => SmpConfigurationService.CreateAsync(null));
            }

            [Fact]
            public async Task ThrowsBusinessException_WhenKeyIsProvidedWithoutFileName()
            {
                // Arrange
                _smpConfiguration.EncryptPublicKeyCertificateName = null;
                _smpConfiguration.EncryptPublicKeyCertificate = EncryptionCertificateContents;

                // Act
                var exception = await Assert.ThrowsAsync<BusinessException>(() => SmpConfigurationService.CreateAsync(_smpConfiguration));

                // Assert
                Assert.Equal("EncryptPublicKeyCertificateName needs to be provided when EncryptPublicKeyCertificate is not empty!", exception.Message);
            }
        }

        public class Update : SmpConfigurationServiceTest
        {
            [Fact]
            public async Task ThrowsArgumentException_WhenIdIsInvalid()
            {
                // Act / Assert
                await Assert.ThrowsAsync<ArgumentException>(() => SmpConfigurationService.UpdateAsync(0, new SmpConfigurationDetail()));
            }

            [Fact]
            public async Task ThrowsArgumentNullException_WhenSmpConfigurationIsNull()
            {
                // Act / Assert
                await Assert.ThrowsAsync<ArgumentNullException>(() => SmpConfigurationService.UpdateAsync(1, null));
            }

            [Fact]
            public async Task ThrowsNotFoundException_WhenSmpConfigurationDoesntExist()
            {
                SmpConfigurationDetail fixture = CreateFixture();

                // Act / Assert
                await Assert.ThrowsAsync<NotFoundException>(() => SmpConfigurationService.UpdateAsync(int.MaxValue, fixture));
            }

            private static SmpConfigurationDetail CreateFixture()
            {
                return new SmpConfigurationDetail
                {
                    Action = "Action",
                    EncryptAlgorithm = "EncryptAlgorithm",
                    EncryptAlgorithmKeySize = 10,
                    EncryptKeyDigestAlgorithm = "EncryptKeyDigestAlgorithm",
                    EncryptKeyMgfAlorithm = "EncryptKeyMgfAlorithm",
                    EncryptKeyTransportAlgorithm = "EncryptKeyTransportAlgorithm",
                    EncryptPublicKeyCertificate = null,
                    EncryptionEnabled = true,
                    FinalRecipient = "FinalRecipient",
                    PartyRole = "PartyRole",
                    PartyType = "PartyType",
                    ServiceType = "ServiceType",
                    ServiceValue = "ServiceValue",
                    TlsEnabled = true,
                    ToPartyId = "ToPartyId",
                    Url = "Url"
                };
            }

            [Fact]
            public async Task UpdatesExisting_WhenParametersAreValid()
            {
                // Arrange
                await SmpConfigurationService.CreateAsync(_smpConfiguration);
                var existingConfiguration = await DbContext.SmpConfigurations.FirstOrDefaultAsync();
                Assert.NotNull(existingConfiguration);

                existingConfiguration.Action = Guid.NewGuid().ToString();

                // Act
                await SmpConfigurationService.UpdateAsync(existingConfiguration.Id, CreateFixture());

                // Assert
                var updatedFromDatabase = await DbContext.SmpConfigurations.FirstOrDefaultAsync(smpConfiguration =>
                    smpConfiguration.Id == existingConfiguration.Id);
                Assert.NotNull(updatedFromDatabase);

                Assert.Equal(JsonConvert.SerializeObject(existingConfiguration),
                    JsonConvert.SerializeObject(updatedFromDatabase));
            }

            [Fact]
            public async Task ThrowsBusinessException_WhenKeyIsProvidedWithoutFileName()
            {
                // Arrange
                SmpConfigurationDetail dbSmpConfiguration = await SmpConfigurationService.CreateAsync(_smpConfiguration);
                Assert.NotNull(dbSmpConfiguration.Id);

                _smpConfiguration.EncryptPublicKeyCertificate = Convert.ToBase64String(new byte[] { 1, 2, 3 });

                SmpConfigurationDetail smpConfiguration = CreateFixture();
                smpConfiguration.EncryptPublicKeyCertificate = "not empty";

                // Act
                var exception = await Assert.ThrowsAsync<BusinessException>(() => SmpConfigurationService.UpdateAsync(dbSmpConfiguration.Id.Value, smpConfiguration));

                // Assert
                Assert.Equal("EncryptPublicKeyCertificateName needs to be provided when EncryptPublicKeyCertificate is not empty!", exception.Message);
            }
        }

        public class Delete : SmpConfigurationServiceTest
        {
            [Fact]
            public async Task DeletesExisting_WithValidSmpConfiguration()
            {
                // Act
                await SmpConfigurationService.CreateAsync(_smpConfiguration);

                // Assert
                var smpConfigurationFromDatabase = await DbContext.SmpConfigurations.FirstOrDefaultAsync();
                Assert.NotNull(smpConfigurationFromDatabase);

                // Create new DbContext & SmpConfigurationService to make sure that the created SmpConfiguration entity is not in the context anymore
                await CreateNewDbContext()
                    .CreateNewSmpConfigurationService()
                    .SmpConfigurationService
                    .DeleteAsync(smpConfigurationFromDatabase.Id);

                var configurationCount = await DbContext.SmpConfigurations.CountAsync();
                Assert.Equal(0, configurationCount);
            }

            [Fact]
            public async Task ThrowsArgumentException_WhenIdIsInvalid()
            {
                // Act / Arrange
                await Assert.ThrowsAsync<ArgumentException>(() => SmpConfigurationService.DeleteAsync(0));
            }

            [Fact]
            public async Task ThrowsNotFoundException_WhenSmpConfigurationDoesntExist()
            {
                // Act / Arrange
                await Assert.ThrowsAsync<NotFoundException>(() => SmpConfigurationService.DeleteAsync(int.MaxValue));
            }
        }
    }
}