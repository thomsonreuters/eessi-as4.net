using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Setup submit tool dependencies
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.Pmodes.IPmodeSetup" />
    public class SubmitToolSetup : ISubmitToolSetup
    {
        /// <summary>
        /// Runs the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public void Run(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<ISubmitMessageCreator, SubmitMessageCreator>();

            services.AddSingleton<IPayloadHandler, PayloadHttpServiceHandler>();
            services.AddSingleton<IPayloadHandler, SimulatePayloadServiceHandler>();
            services.AddSingleton<IPayloadHandler, FilePayloadHandler>();

            services.AddSingleton<IMessageHandler, MshMessageHandler>();
            services.AddSingleton<IMessageHandler, FileMessageHandler>();

            services.Configure<SubmitToolOptions>(configuration.GetSection("SubmitTool"));
        }
    }
}