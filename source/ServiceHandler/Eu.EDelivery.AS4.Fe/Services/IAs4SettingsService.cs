using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;

namespace Eu.EDelivery.AS4.Fe.Services
{
    public interface IAs4SettingsService : IModular
    {
        Task<Settings> GetSettings();
        Task SaveBaseSettings(BaseSettings settings);
        Task SaveCustomSettings(CustomSettings settings);
        Task SaveDatabaseSettings(SettingsDatabase settings);
    }
}