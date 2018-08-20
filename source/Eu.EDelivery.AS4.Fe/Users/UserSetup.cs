using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.Users
{
    /// <summary>
    /// Implementation of the user setup module
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Users.IUserSetup" />
    public class UserSetup : IUserSetup
    {
        /// <summary>
        /// Runs the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddScoped<IUserService, UserService>();
        }
    }
}