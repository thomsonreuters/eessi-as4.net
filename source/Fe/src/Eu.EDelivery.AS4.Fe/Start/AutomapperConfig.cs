using AutoMapper;
using Eu.EDelivery.AS4.Fe.Models;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Eu.EDelivery.AS4.Model.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Start
{
    public static class AutomapperConfig
    {
        public static void AddAutoMapper(this IServiceCollection services)
        {
            var config = MapperConfiguration();
            services.AddSingleton(config.CreateMapper());
        }

        public static MapperConfiguration MapperConfiguration()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BaseSettings, Model.Internal.Settings>();
                cfg.CreateMap<Model.Internal.Settings, BaseSettings>();
                cfg.CreateMap<CustomSettings, Model.Internal.Settings>();
                cfg.CreateMap<CustomSettings, CustomSettings>();
                cfg.CreateMap<SettingsDatabase, SettingsDatabase>();
                cfg.CreateMap<SettingsAgent, SettingsAgent>();
                cfg.CreateMap<SendingPmode, SendingPmode>();
                cfg.CreateMap<ReceivingPmode, ReceivingPmode>();
            });
        }
    }
}