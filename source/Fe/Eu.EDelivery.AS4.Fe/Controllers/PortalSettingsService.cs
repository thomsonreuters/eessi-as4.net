using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Fe.Controllers
{
    /// <summary>
    /// Implementation to manage portal settings using appsettings.json
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Controllers.IPortalSettingsService" />
    public class PortalSettingsService : IPortalSettingsService
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IOptions<PortalSettings> settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingsService" /> class.
        /// </summary>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        /// <param name="applicationDbContext">The application database context.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="settings">The settings.</param>
        public PortalSettingsService(IHostingEnvironment hostingEnvironment, ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager, IOptions<PortalSettings> settings)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.applicationDbContext = applicationDbContext;
            this.userManager = userManager;
            this.settings = settings;
        }

        /// <summary>
        /// Saves the specified save.
        /// </summary>
        /// <param name="save">The save.</param>
        /// <returns></returns>
        public async Task Save(PortalSettings save)
        {
            var path = hostingEnvironment.ContentRootPath;
            var result = JsonConvert.SerializeObject(save);
            var fileName = Path.Combine(path, $"appsettings.{hostingEnvironment.EnvironmentName}.json");
            if (!File.Exists(fileName)) fileName = $"{path}appsettings.json";

            File.WriteAllText(fileName, result);
            await Task.FromResult(0);
        }

        /// <summary>
        /// Determines whether the portal is in setup state
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsSetup()
        {
            var hasUsers = await applicationDbContext.Users.AnyAsync();
            return hasUsers;
        }

        /// <summary>
        /// Saves the setup.
        /// </summary>
        /// <returns></returns>
        public async Task SaveSetup(Setup setup)
        {
            // Create the admin & readonly user 
            await CreateUsers(setup);

            settings.Value.Authentication.JwtOptions.Key = setup.JwtKey;
            await Save(settings.Value);
        }

        private async Task CreateUsers(Setup setup)
        {
            applicationDbContext.Database.EnsureCreated();
            applicationDbContext.SaveChanges();

            var adminUser = new ApplicationUser { UserName = "admin" };
            var readonlyUser = new ApplicationUser { UserName = "readonly" };

            await userManager.CreateAsync(adminUser, setup.AdminPassword);
            await userManager.CreateAsync(readonlyUser, setup.ReadonlyPassword);

            userManager.AddClaimsAsync(adminUser, new[] { new Claim(ClaimTypes.Role, Roles.Admin) }).Wait();
            userManager.AddClaimsAsync(readonlyUser, new[] { new Claim(ClaimTypes.Role, Roles.Readonly) }).Wait();
        }
    }
}