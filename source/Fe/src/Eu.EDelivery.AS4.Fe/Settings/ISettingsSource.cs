using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Modules;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public interface ISettingsSource : IModular
    {
        Task<Model.Internal.Settings> Get();
        Task Save(Model.Internal.Settings settings);
    }
}