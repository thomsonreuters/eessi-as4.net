using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EnsureThat;
using Eu.EDelivery.AS4.Common;
using Microsoft.EntityFrameworkCore;

namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     Implementation of <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration.ISmpConfigurationService" />
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SmpConfiguration.ISmpConfigurationService" />
    public class SmpConfigurationService : ISmpConfigurationService
    {
        private readonly DatastoreContext _datastoreContext;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SmpConfigurationService" /> class.
        /// </summary>
        /// <param name="datastoreContext">The datastore context.</param>
        /// <param name="mapper">Instance of <see cref="IMapper" /></param>
        public SmpConfigurationService(DatastoreContext datastoreContext, IMapper mapper)
        {
            _datastoreContext = datastoreContext;
            _mapper = mapper;
        }

        /// <summary>
        ///     Get all SMP configurations
        /// </summary>
        /// <returns>
        ///     Collection containing all <see cref="N:Eu.EDelivery.AS4.Fe.SmpConfiguration" />
        /// </returns>
        public async Task<IEnumerable<Entities.SmpConfiguration>> GetAll()
        {
            var configurations = await _datastoreContext.SmpConfigurations.ToListAsync();
            return configurations;
        }

        /// <summary>
        ///     Create an e new <see cref="N:Eu.EDelivery.AS4.Fe.SmpConfiguration" />
        /// </summary>
        /// <param name="smpConfiguration">The SMP configuration.</param>
        /// <returns></returns>
        public async Task<Entities.SmpConfiguration> Create(SmpConfiguration smpConfiguration)
        {
            EnsureArg.IsNotNull(smpConfiguration, nameof(smpConfiguration));
            ValidateSmpConfiguration(smpConfiguration);

            var configuration = _mapper.Map<Entities.SmpConfiguration>(smpConfiguration);
            ParseKeyCertificate(smpConfiguration, configuration);

            await _datastoreContext.SmpConfigurations.AddAsync(configuration);
            await _datastoreContext.SaveChangesAsync();

            return configuration;
        }

        /// <summary>
        ///     Update an existing <see cref="N:Eu.EDelivery.AS4.Fe.SmpConfiguration" /> by id
        /// </summary>
        /// <param name="id">The id of the SmpConfiguration</param>
        /// <param name="smpConfiguration">SMP configuration data to be updated</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task Update(long id, SmpConfiguration smpConfiguration)
        {
            EnsureArg.IsNotNull(smpConfiguration, nameof(smpConfiguration));
            EnsureArg.IsTrue(id > 0, nameof(id));
            ValidateSmpConfiguration(smpConfiguration);

            var existing =
                await _datastoreContext.SmpConfigurations.FirstOrDefaultAsync(configuration => configuration.Id == id);
            if (existing == null) throw new NotFoundException($"No smp configuration with id {id} found.");

            _mapper.Map(smpConfiguration, existing);
            ParseKeyCertificate(smpConfiguration, existing);

            _datastoreContext.Entry(existing).State = EntityState.Modified;
            await _datastoreContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Delete an existing <see cref="N:Eu.EDelivery.AS4.Fe.SmpConfiguration" /> by id
        /// </summary>
        /// <param name="id">The id of the <see cref="N:Eu.EDelivery.AS4.Fe.SmpConfiguration" /></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task Delete(long id)
        {
            EnsureArg.IsTrue(id > 0, nameof(id));

            var exists = await _datastoreContext.SmpConfigurations.CountAsync(configuration => configuration.Id == id);
            if (exists == 0) throw new NotFoundException($"No smp configuration with id {id} found");

            _datastoreContext.SmpConfigurations.Remove(new Entities.SmpConfiguration
            {
                Id = id
            });

            await _datastoreContext.SaveChangesAsync();
        }

        private void ParseKeyCertificate(SmpConfiguration smpConfiguration, Entities.SmpConfiguration existing)
        {
            if (string.IsNullOrEmpty(smpConfiguration.EncryptPublicKeyCertificate))
            {
                return;
            }
            // Convert the certificate string to a byte array
            var split = smpConfiguration.EncryptPublicKeyCertificate.Split(',');
            existing.EncryptPublicKeyCertificate =
                Convert.FromBase64String(split[split.Length > 1 ? 1 : 0]);
        }

        private void ValidateSmpConfiguration(SmpConfiguration smpConfiguration)
        {
            EnsureArg.IsTrue(smpConfiguration.EncryptAlgorithmKeySize >= 0,
                nameof(smpConfiguration.EncryptAlgorithmKeySize));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.PartyRole, nameof(smpConfiguration.PartyRole));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.PartyType, nameof(smpConfiguration.PartyType));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.ToPartyId, nameof(smpConfiguration.ToPartyId));
            EnsureArg.IsNotNullOrWhiteSpace(smpConfiguration.Url, nameof(smpConfiguration.Url));
            if (!string.IsNullOrEmpty(smpConfiguration.EncryptPublicKeyCertificate) && string.IsNullOrEmpty(smpConfiguration.EncryptPublicKeyCertificateName))
            {
                throw new BusinessException("EncryptPublicKeyCertificateName needs to be provided when EncryptPublicKeyCertificate is not empty!");
            }
        }
    }
}