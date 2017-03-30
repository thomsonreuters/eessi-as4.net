using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Eu.EDelivery.AS4.Fe.Logging
{
    public class LoggerSetup : ILoggerSetup
    {
        private readonly IHostingEnvironment env;
        private readonly ILoggerFactory loggerFactory;

        public LoggerSetup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            this.env = env;
            this.loggerFactory = loggerFactory;
        }

        public void Run(IApplicationBuilder app)
        {
            //env.ConfigureNLog("nlog.config");
            //loggerFactory.AddNLog();
        }
    }
}