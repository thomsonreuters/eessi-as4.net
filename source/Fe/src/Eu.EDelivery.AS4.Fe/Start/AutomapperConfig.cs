using AutoMapper;
using Eu.EDelivery.AS4.Fe.AS4Model;
using Eu.EDelivery.AS4.Fe.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Start
{
    public static class AutomapperConfig
    {
        public static void AddAutoMapper(this IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BaseSettings, AS4Model.Settings>();
                cfg.CreateMap<AS4Model.Settings, BaseSettings>();
                cfg.CreateMap<CustomSettings, AS4Model.Settings>();
                cfg.CreateMap<CustomSettings, CustomSettings>();
                cfg.CreateMap<SettingsDatabase, SettingsDatabase>();
                cfg.CreateMap<SettingsAgent, SettingsAgent>();
            });

            services.AddSingleton(config.CreateMapper());
        }
    }
}