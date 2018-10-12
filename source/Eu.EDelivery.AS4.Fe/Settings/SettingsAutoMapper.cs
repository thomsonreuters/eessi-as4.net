using AutoMapper;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Fe.Settings
{
    public class SettingsAutoMapper : Profile
    {
        public SettingsAutoMapper()
        {
            CreateMap<BaseSettings, Model.Internal.Settings>();
            CreateMap<Model.Internal.Settings, BaseSettings>();
            CreateMap<SettingsPullSend, Model.Internal.Settings>();
            CreateMap<SettingsPullSend, SettingsPullSend>();
            CreateMap<CustomSettings, Model.Internal.Settings>();
            CreateMap<CustomSettings, CustomSettings>();
            CreateMap<SettingsDatabase, SettingsDatabase>();
            CreateMap<AgentSettings, AgentSettings>();
        }
    }
}