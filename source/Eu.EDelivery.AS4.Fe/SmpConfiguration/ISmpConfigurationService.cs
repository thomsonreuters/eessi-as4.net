using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.SmpConfiguration.Model;

namespace Eu.EDelivery.AS4.Fe.SmpConfiguration
{
    /// <summary>
    ///     Interface for managing SMP configurations.
    /// </summary>
    public interface ISmpConfigurationService
    {
        /// <summary>
        ///     Get all SMP configurations
        /// </summary>
        /// <returns>Collection containing all <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" /></returns>
        Task<IEnumerable<SmpConfigurationRecord>> GetRecordsAsync();

        /// <summary>
        ///     Get SMP configuration by identifier
        /// </summary>
        /// <returns>
        ///     Matched <see cref="N:Eu.EDelivery.AS4.Fe.SmpConfiguration" /> if found
        /// </returns>
        Task<SmpConfigurationDetail> GetByIdAsync(int id);

        /// <summary>
        ///     Create an e new <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" />
        /// </summary>
        /// <param name="detail">The SMP configuration.</param>
        Task<SmpConfigurationDetail> CreateAsync(SmpConfigurationDetail detail);

        /// <summary>
        ///     Update an existing <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" /> by id
        /// </summary>
        /// <param name="id">The id of the SmpConfiguration</param>
        /// <param name="detail">SMP configuration data to be updated</param>
        Task UpdateAsync(long id, SmpConfigurationDetail detail);

        /// <summary>
        ///     Delete an existing <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" /> by id
        /// </summary>
        /// <param name="id">The id of the <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration"/></param>
        Task DeleteAsync(long id);
    }
}