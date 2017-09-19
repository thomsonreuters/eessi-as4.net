using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Models;

namespace Eu.EDelivery.AS4.Fe.Controllers
{
    /// <summary>
    /// Interface to be implemented to manage portal settings
    /// </summary>
    public interface IPortalSettingsService
    {
        /// <summary>
        /// Saves the specified save.
        /// </summary>
        /// <param name="save">The save.</param>
        /// <returns></returns>
        Task Save(PortalSettings save);

        /// <summary>
        /// Determines whether the portal is in setup state
        /// </summary>
        /// <returns></returns>
        Task<bool> IsSetup();

        /// <summary>
        /// Saves the setup.
        /// </summary>
        /// <returns></returns>
        Task SaveSetup(Setup setup);
    }
}