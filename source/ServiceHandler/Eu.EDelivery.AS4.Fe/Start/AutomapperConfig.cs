using AutoMapper;
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
                cfg.CreateMap<BaseSettings, Settings>();
                cfg.CreateMap<Settings, BaseSettings>();
                cfg.CreateMap<CustomSettings, Settings>();
                cfg.CreateMap<CustomSettings, CustomSettings>();
                cfg.CreateMap<SettingsDatabase, SettingsDatabase>();
            });

            services.AddSingleton(config.CreateMapper());
        }
    }
}