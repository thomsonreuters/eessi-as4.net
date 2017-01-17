using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Swagger
{
    public class SwaggerSetup : ISwaggerSetup
    {
        public void Run(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUi();
        }

        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSwaggerGen();
        }
    }
}