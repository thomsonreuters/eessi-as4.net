using System.Threading.Tasks;

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
    }
}