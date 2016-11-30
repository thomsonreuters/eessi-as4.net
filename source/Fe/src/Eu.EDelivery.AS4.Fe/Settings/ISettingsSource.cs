using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Services;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public interface ISettingsSource : IModular
    {
        Task<AS4Model.Settings> Get();
        Task Save(AS4Model.Settings settings);
    }
}