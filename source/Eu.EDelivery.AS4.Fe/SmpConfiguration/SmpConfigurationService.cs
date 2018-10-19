using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EnsureThat;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.SmpConfiguration.Model;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     Implementation of <see cref="ISmpConfigurationService" />
    /// </summary>
    /// <seealso cref="ISmpConfigurationService" />
    public class SmpConfigurationService : ISmpConfigurationService
    {
        private const string Base64CerHeader = "data:application/x-x509-ca-cert;base64,";
        private const string Base64PkcsHeader = "data:application/x-pkcs12;base64,";

        private readonly DatastoreContext _datastoreContext;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmpConfigurationService" /> class.
        /// </summary>
        /// <param name="datastoreContext">The datastore context.</param>
        /// <param name="mapper">Instance of <see cref="IMapper" /></param>
        public SmpConfigurationService(DatastoreContext datastoreContext, IMapper mapper)
        {
            if (datastoreContext == null)
            {
                throw new ArgumentNullException(nameof(datastoreContext));
            }

            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            _datastoreContext = datastoreContext;
            _mapper = mapper;
        }

        /// <summary>
        ///     Get all SMP configurations
        /// </summary>
        /// <returns>
        ///     Collection containing all <see cref="N:Eu.EDelivery.AS4.Fe.SmpConfiguration.Model.SmpConfigurationRecord" />
        /// </returns>
        public async Task<IEnumerable<SmpConfigurationRecord>> GetRecordsAsync()
        {
            List<Entities.SmpConfiguration> configurations = await _datastoreContext.SmpConfigurations.ToListAsync();
            return configurations.Select(smp => new SmpConfigurationRecord
            {
                Id = smp.Id,
                Action = smp.Action,
                Url = smp.Url,
                ServiceType = smp.ServiceType,
                ServiceValue = smp.ServiceValue,
                TlsEnabled = smp.TlsEnabled,
                ToPartyId = smp.ToPartyId,
                PartyRole = smp.PartyRole,
                EncryptionEnabled = smp.EncryptionEnabled,
                FinalRecipient = smp.FinalRecipient,
                PartyType = smp.PartyType
            });
        }

        /// <summary>
        ///     Get SMP configuration by identifier
        /// </summary>
        /// <returns>
        ///     Matched <see cref="N:Eu.EDelivery.AS4.Fe.Model.SmpConfigurationDetail" /> if found
        /// </returns>
        public async Task<SmpConfigurationDetail> GetByIdAsync(int id)
        {
            Entities.SmpConfiguration entity = 
                await _datastoreContext
                      .SmpConfigurations
                      .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null)
            {
                return null;
            }

            return ToDetail(entity);
        }

        /// <summary>
        ///     Create an e new <see cref="N:Eu.EDelivery.AS4.Fe.Model.SmpConfigurationDetail" />
        /// </summary>
        /// <param name="detail">The SMP configuration.</param>
        /// <returns></returns>
        public async Task<SmpConfigurationDetail> CreateAsync(SmpConfigurationDetail detail)
        {
            EnsureArg.IsNotNull(detail, nameof(detail));

            ValidateSmpConfiguration(detail);

            var configuration = _mapper.Map<Entities.SmpConfiguration>(detail);
            configuration.EncryptPublicKeyCertificate =
                DeserializePublicKeyCertificate(detail.EncryptPublicKeyCertificate);

            await _datastoreContext.SmpConfigurations.AddAsync(configuration);
            await _datastoreContext.SaveChangesAsync();

            return ToDetail(configuration);
        }

        private static SmpConfigurationDetail ToDetail(Entities.SmpConfiguration s)
        {
            return new SmpConfigurationDetail
            {
                Id = s.Id,
                Action = s.Action,
                ServiceType = s.ServiceType,
                ServiceValue = s.ServiceValue,
                FinalRecipient = s.FinalRecipient,
                ToPartyId = s.ToPartyId,
                PartyRole = s.PartyRole,
                TlsEnabled = s.TlsEnabled,
                Url = s.Url,
                PartyType = s.PartyType,
                EncryptionEnabled = s.EncryptionEnabled,
                EncryptAlgorithm = s.EncryptAlgorithm,
                EncryptAlgorithmKeySize = s.EncryptAlgorithmKeySize,
                EncryptKeyDigestAlgorithm = s.EncryptKeyDigestAlgorithm,
                EncryptKeyMgfAlorithm = s.EncryptKeyMgfAlorithm,
                EncryptKeyTransportAlgorithm = s.EncryptKeyTransportAlgorithm,
                EncryptPublicKeyCertificate = 
                    s.EncryptPublicKeyCertificate == null 
                        ? null 
                        : Convert.ToBase64String(s.EncryptPublicKeyCertificate),
                EncryptPublicKeyCertificateName = s.EncryptPublicKeyCertificateName
            };
        }

        /// <summary>
        ///     Update an existing <see cref="N:Eu.EDelivery.AS4.Fe.Model.SmpConfigurationDetail" /> by id
        /// </summary>
        /// <param name="id">The id of the SmpConfiguration</param>
        /// <param name="detail">SMP configuration data to be updated</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task UpdateAsync(long id, SmpConfigurationDetail detail)
        {
            EnsureArg.IsNotNull(detail, nameof(detail));
            
            EnsureArg.IsTrue(id > 0, nameof(id));
            ValidateSmpConfiguration(detail);

            Entities.SmpConfiguration existing = 
                await _datastoreContext
                      .SmpConfigurations
                      .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
            {
                throw new NotFoundException($"No smp configuration with id {id} found.");
            }

            _mapper.Map(detail, existing);
            existing.EncryptPublicKeyCertificate = 
                DeserializePublicKeyCertificate(detail.EncryptPublicKeyCertificate);

            _datastoreContext.Entry(existing).State = EntityState.Modified;
            await _datastoreContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Delete an existing <see cref="N:Eu.EDelivery.AS4.Fe.Model.SmpConfigurationDetail" /> by id
        /// </summary>
        /// <param name="id">The id of the <see cref="N:Eu.EDelivery.AS4.Fe.Model.SmpConfigurationDetail" /></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task DeleteAsync(long id)
        {
            EnsureArg.IsTrue(id > 0, nameof(id));

            int exists = 
                await _datastoreContext
                      .SmpConfigurations
                      .CountAsync(configuration => configuration.Id == id);

            if (exists == 0)
            {
                throw new NotFoundException($"No smp configuration with id {id} found");
            }

            _datastoreContext
                .SmpConfigurations
                .Remove(new Entities.SmpConfiguration { Id = id });

            await _datastoreContext.SaveChangesAsync();
        }

        private static byte[] DeserializePublicKeyCertificate(string base64)
        {
            if (!String.IsNullOrEmpty(base64))
            {
                if (base64.StartsWith(Base64CerHeader)
                    || base64.StartsWith(Base64PkcsHeader))
                {
                    // Convert the certificate string to a byte array
                    string[] split = base64.Split(',');
                    return Convert.FromBase64String(split[split.Length > 1 ? 1 : 0]);
                }

                return Convert.FromBase64String(base64);
            }

            return null;
        }

        private void ValidateSmpConfiguration(SmpConfigurationDetail smpConfiguration)
        {
            EnsureArg.IsTrue(smpConfiguration.EncryptAlgorithmKeySize >= 0, nameof(smpConfiguration.EncryptAlgorithmKeySize));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.PartyRole, nameof(smpConfiguration.PartyRole));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.PartyType, nameof(smpConfiguration.PartyType));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.ToPartyId, nameof(smpConfiguration.ToPartyId));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.Url, nameof(smpConfiguration.Url));

            if (!String.IsNullOrEmpty(smpConfiguration.EncryptPublicKeyCertificate) 
                && String.IsNullOrEmpty(smpConfiguration.EncryptPublicKeyCertificateName))
            {
                throw new BusinessException(
                    "EncryptPublicKeyCertificateName needs to be provided when EncryptPublicKeyCertificate is not empty!");
            }
        }
    }
}