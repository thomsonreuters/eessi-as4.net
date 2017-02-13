using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Automapper
{
    public class AutomapperSetup : IAutomapperSetup
    {
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies().ToArray();
            var allTypes = assembliesToScan.SelectMany(a => a.ExportedTypes).ToArray();
            var profiles = allTypes
                .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                .Where(t => !t.GetTypeInfo().IsAbstract);

            Mapper.Initialize(cfg =>
            {
                foreach (var profile in profiles)
                {
                    cfg.AddProfile(profile);
                }
            });

            services.AddSingleton(Mapper.Configuration);
            services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService));
        }
    }
}