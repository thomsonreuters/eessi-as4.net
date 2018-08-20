using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Linq;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Fe.Tests.TestData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class ScannerTests
    {
        private readonly Scanner scanner = new Scanner();

        public class Register : ScannerTests
        {
            private readonly IServiceCollection serviceCollection = Substitute.For<IServiceCollection>();
            private readonly Assembly localAssembly = typeof(TestSettings).GetTypeInfo().Assembly;

            [Fact]
            public void Loads_Internal_Implementation_When_No_Config_Is_Set()
            {
                // Act
                scanner.Register<IModular>(serviceCollection, typeof(As4SettingsService).GetTypeInfo().Assembly.DefinedTypes.ToList(), Enumerable.Empty<TypeInfo>().ToList());

                // Assert
                serviceCollection.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IAs4SettingsService) && x.ImplementationType == typeof(As4SettingsService)));
            }

            [Fact]
            public void Loads_External_Implementation_When_Set_In_Config()
            {
                // Setup
                var config = new Dictionary<string, string>()
                {
                    { "Eu.EDelivery.AS4.Fe.Services.IAs4SettingsService", "Eu.EDelivery.AS4.Fe.Tests" }
                };
                    
                // Act
                scanner.Register<IModular>(serviceCollection, typeof(As4SettingsService).GetTypeInfo().Assembly.DefinedTypes.ToList(), localAssembly.DefinedTypes.ToList(), config);

                // Assert
                serviceCollection.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IAs4SettingsService) && x.ImplementationType == typeof(TestSettings)));
            }

            [Fact]
            public void Loads_IRunAtServicesStartup()
            {
                // Setup
                var config = new Dictionary<string, string>();

                // Act
                scanner.Register<IRunAtServicesStartup>(serviceCollection, typeof(IRunAtServicesStartup).GetTypeInfo().Assembly.DefinedTypes.ToList(), localAssembly.DefinedTypes.ToList(), config);

                // Assert
                serviceCollection.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IRunAtServicesStartup) && x.ImplementationType == typeof(TestRunAtStartup)));
            }
        }

        public class TestRunAtStartup : IRunAtServicesStartup
        {
            [ExcludeFromCodeCoverage]
            public void Run(IServiceCollection services, IConfigurationRoot configuration)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
