using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingsService"/> class.
        /// </summary>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        public PortalSettingsService(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
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
    }
}