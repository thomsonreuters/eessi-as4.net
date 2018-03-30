using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<IEnumerable<Entities.SmpConfiguration>> GetAll();

        /// <summary>
        ///     Create an e new <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" />
        /// </summary>
        /// <param name="smpConfiguration">The SMP configuration.</param>
        Task<Entities.SmpConfiguration> Create(SmpConfiguration smpConfiguration);

        /// <summary>
        ///     Update an existing <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" /> by id
        /// </summary>
        /// <param name="id">The id of the SmpConfiguration</param>
        /// <param name="smpConfiguration">SMP configuration data to be updated</param>
        Task Update(long id, SmpConfiguration smpConfiguration);

        /// <summary>
        ///     Delete an existing <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration" /> by id
        /// </summary>
        /// <param name="id">The id of the <see cref="Eu.EDelivery.AS4.Fe.SmpConfiguration"/></param>
        Task Delete(long id);
    }
}