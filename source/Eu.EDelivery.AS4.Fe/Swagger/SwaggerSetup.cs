using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace Eu.EDelivery.AS4.Fe.Swagger
{
    /// <summary>
    /// Setup swagger
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Swagger.ISwaggerSetup" />
    public class SwaggerSetup : ISwaggerSetup
    {
        private string AssemblyVersion => GetType().GetTypeInfo().Assembly.GetName().Version.ToString();


        /// <summary>
        /// Runs the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Run(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AS4 FE api");
            });
        }

        /// <summary>
        /// Runs the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.IncludeXmlComments(GetXmlCommentsPath());
                options.SwaggerDoc("v1", new Info { Title = "AS4 FE Api", Version = AssemblyVersion });
                options.OperationFilter<FileUploadOperation>();
            });
        }

        private string GetXmlCommentsPath()
        {
            const string xml = "Eu.EDelivery.AS4.Fe.xml";
            var app = PlatformServices.Default.Application;
            var binPath = Path.Combine(app.ApplicationBasePath, "bin", xml);
            return File.Exists(binPath) ? binPath : Path.Combine(app.ApplicationBasePath, xml);
        }
    }
}