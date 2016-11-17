using System.Collections.Generic;
using System.Reflection;
using Eu.EDelivery.AS4.Fe.Modules;
using Eu.EDelivery.AS4.Fe.Services;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.Tests
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
                scanner.Register(serviceCollection, typeof(As4SettingsService).GetTypeInfo().Assembly, new [] { localAssembly }, new Dictionary<string, string>());

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
                scanner.Register(serviceCollection, typeof(As4SettingsService).GetTypeInfo().Assembly, new[] { localAssembly }, config);

                // Assert
                serviceCollection.Add(Arg.Is<ServiceDescriptor>(x => x.ServiceType == typeof(IAs4SettingsService) && x.ImplementationType == typeof(TestSettings)));
            }
        }
    }
}
