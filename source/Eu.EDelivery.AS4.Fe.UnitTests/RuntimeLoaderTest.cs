using System.IO;
using Eu.EDelivery.AS4.Fe.Runtime;
using Eu.EDelivery.AS4.Fe.Settings;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    public class RuntimeLoaderTest
    {
        [Fact]
        public void Types_Should_Be_Loaded_From_The_Assemblies_At_Given_Directory()
        {
            // Arrange
            var options = Substitute.For<IOptions<ApplicationSettings>>();
            options.Value.Returns(new ApplicationSettings()
            {
                Runtime = Directory.GetCurrentDirectory()
            });

            // Act
            var loader = RuntimeLoader.Initialize(options);

            // Assert
            Assert.NotEmpty(loader.Receivers);
            Assert.NotEmpty(loader.Transformers);
            Assert.NotEmpty(loader.Steps);
            Assert.NotEmpty(loader.AttachmentUploaders);
            Assert.NotEmpty(loader.CertificateRepositories);
            Assert.NotEmpty(loader.DeliverSenders);
            Assert.NotEmpty(loader.DynamicDiscoveryProfiles);
            Assert.NotEmpty(loader.NotifySenders);
            Assert.NotEmpty(loader.MetaData);
        }
    }
}