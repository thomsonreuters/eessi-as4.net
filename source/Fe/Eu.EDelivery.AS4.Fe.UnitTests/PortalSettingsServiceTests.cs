using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using (var store = new ApplicationDbContext(options))
            {
                store.Database.EnsureCreated();
            }
            var context = new ApplicationDbContext(options);

            var hostingEnvironment = Substitute.For<IHostingEnvironment>();
            hostingEnvironment.ContentRootPath = @"c:\temp\";
            var userManager = Substitute.For<UserManager<ApplicationUser>>(
                Substitute.For<IUserStore<ApplicationUser>>(),
                Substitute.For<IOptions<IdentityOptions>>(),
                Substitute.For<IPasswordHasher<ApplicationUser>>(),
                new IUserValidator<ApplicationUser>[0],
                new IPasswordValidator<ApplicationUser>[0],
                Substitute.For<ILookupNormalizer>(),
                Substitute.For<IdentityErrorDescriber>(),
                Substitute.For<IServiceProvider>(),
                Substitute.For<ILogger<UserManager<ApplicationUser>>>());
            var settings = Substitute.For<IOptions<PortalSettings>>();

            runtimeSettingsService = new PortalSettingsService(hostingEnvironment, context, userManager, settings);
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
                    Port = "test"
                };

                await runtimeSettingsService.Save(test);
            }
        }
    }
}