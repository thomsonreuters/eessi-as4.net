using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Controllers
{
    /// <summary>
    /// <see cref="IHostingEnvironment"/> implementation to 'Stub' the Hosting Environment.
    /// </summary>
    public class StubHostingEnvironment : IHostingEnvironment
    {
        /// <summary>
        /// Gets or sets the name of the environment. This property is automatically set by the host to the value
        /// of the "ASPNETCORE_ENVIRONMENT" environment variable.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets the name of the application. This property is automatically set by the host to the assembly containing
        /// the application entry point.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the absolute path to the directory that contains the web-servable application content files.
        /// </summary>
        public string WebRootPath { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="T:Microsoft.Extensions.FileProviders.IFileProvider" /> pointing at <see cref="P:Microsoft.AspNetCore.Hosting.IHostingEnvironment.WebRootPath" />.
        /// </summary>
        public IFileProvider WebRootFileProvider { get; set; }

        /// <summary>
        /// Gets or sets the absolute path to the directory that contains the application content files.
        /// </summary>
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets or sets an <see cref="T:Microsoft.Extensions.FileProviders.IFileProvider" /> pointing at <see cref="P:Microsoft.AspNetCore.Hosting.IHostingEnvironment.ContentRootPath" />.
        /// </summary>
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
