using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Controllers;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;
using Xunit;

namespace Eu.EDelivery.AS4.Fe.UnitTests
{
    /// <summary>
    /// Runtime Settings tests
    /// </summary>
    public class PortalSettingsServiceTests
    {
        protected IPortalSettingsService runtimeSettingsService;

        public PortalSettingsServiceTests Setup()
        {
            var hostingEnvironment = Substitute.For<IHostingEnvironment>();
            hostingEnvironment.ContentRootPath = @"c:\temp\";
            runtimeSettingsService = new PortalSettingsService(hostingEnvironment);
            return this;
        }

        public class Save : PortalSettingsServiceTests
        {
            [Fact]
            public async Task SavesTo_AppSettings()
            {
                Setup();

                var test = new PortalSettings()
                {
                    Url = "test"
                };

                await runtimeSettingsService.Save(test);
            }
        }
    }
}