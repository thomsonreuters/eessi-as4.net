using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Owin.Builder;
using Owin;

namespace Eu.EDelivery.AS4.Fe.Monitor
{
    /// <summary>
    /// Middleware to use SignalR with .net core
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Uses the application builder.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="configure">The configure.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAppBuilder(
            this IApplicationBuilder app,
            Action<IAppBuilder> configure)
        {
            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    var appBuilder = new AppBuilder();
                    appBuilder.Properties["builder.DefaultApp"] = next;

                    configure(appBuilder);

                    return appBuilder.Build<Func<IDictionary<string, object>, Task>>();
                });
            });

            return app;
        }

        /// <summary>
        /// Uses the signal r2.
        /// </summary>
        /// <param name="app">The application.</param>
        public static void UseSignalR2(this IApplicationBuilder app)
        {
            app.UseAppBuilder(appBuilder => appBuilder.MapSignalR());
        }
    }
}